using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Zen.Base.Distributed;
using Zen.Base.Extension;
using Zen.Base.Module.Service;

namespace Zen.Base.Maintenance
{
    public class MaintenanceService : IHostedService, IDisposable
    {
        private readonly List<Item> _registeredMaintenanceTasks =
            Resolution.GetClassesByInterface<IMaintenanceTask>()
                .Select(i =>
                {
                    var item = new Item
                    {
                        Type = i,
                        Instance = i.ToInstance<IMaintenanceTask>(),
                        Setup = (MaintenanceTaskSetupAttribute) i.GetMethod("MaintenanceTask")
                                    .GetCustomAttributes(typeof(MaintenanceTaskSetupAttribute), false).FirstOrDefault()
                                ?? new MaintenanceTaskSetupAttribute {Name = "Maintenance Task"}
                    };

                    item.Description = i.FullName + " | " + item.Setup.Name;
                    item.Id = item.Description.StringToGuid().ToString();

                    return item;
                })
                .Where(i => i.Setup.Orchestrated)
                .ToList();
        private Timer _timer;

        #region Implementation of IDisposable

        public void Dispose() { }

        #endregion

        internal class Item
        {
            internal Type Type { get; set; }
            internal IMaintenanceTask Instance { get; set; }
            internal MaintenanceTaskSetupAttribute Setup { get; set; }
            public string Description { get; set; }
            public string Id { get; set; }
        }

        #region Implementation of IHostedService

        public Task StartAsync(CancellationToken cancellationToken)
        {
            if (_registeredMaintenanceTasks.Count == 0) return Task.CompletedTask;

            _timer = new Timer(RunMaintenance, null, TimeSpan.FromSeconds(5), TimeSpan.FromMinutes(5));

            return Task.CompletedTask;
        }

        private readonly object _lockObject = new object();

        private void RunMaintenance(object state)
        {
            if (!Monitor.TryEnter(_lockObject)) return;

            try
            {
                Current.Log.Info($"Orchestrasted Background Maintenance is running {_registeredMaintenanceTasks.Count} maintenance tasks");

                foreach (var item in _registeredMaintenanceTasks)
                {
                    var trackItem = Tracking.Get(item.Id) ?? new Tracking {Id = item.Id, Description = item.Description};

                    var canRun = trackItem.NextRun < DateTime.Now;

                    if (!canRun) continue;

                    var distributedTaskHandler = Factory.GetTicket(item.Description);

                    if (!distributedTaskHandler.CanRun()) continue;

                    Current.Log.KeyValuePair("Maintenance Test", $"[{item.Id}] {item.Description}");

                    distributedTaskHandler.Start();

                    var sw = new Stopwatch();

                    sw.Start();

                    var task = item.Instance.MaintenanceTask();
                    task.Wait();

                    sw.Stop();

                    if (task.Exception != null)
                    {
                        task.Result.Status = Result.EResultStatus.Failed;
                        task.Result.Message = task.Exception.ToSummary();
                    }

                    trackItem.LastResult = task.Result;
                    trackItem.Success = task.Result.Status == Result.EResultStatus.Success;
                    trackItem.Elapsed = sw.Elapsed;

                    trackItem.LastRun = DateTime.Now;
                    trackItem.LastMessage = trackItem.LastResult.Message;

                    if (item.Setup.Cooldown != default) trackItem.NextRun = trackItem.LastRun.Add(item.Setup.Cooldown);

                    trackItem.Save();

                    distributedTaskHandler.Stop();
                }
            }
            finally { Monitor.Exit(_lockObject); }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            Current.Log.Info("Orchestrasted Background Maintenance is stopping.");

            _timer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }

        #endregion
    }
}