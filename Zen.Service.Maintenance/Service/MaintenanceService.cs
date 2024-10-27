using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Zen.Base;
using Zen.Base.Common;
using Zen.Base.Distributed;
using Zen.Base.Extension;
using Zen.Base.Module.Log;
using Zen.Base.Module.Service;
using Zen.Service.Maintenance.Attributes;
using Zen.Service.Maintenance.Model;
using Zen.Service.Maintenance.Shared;
using Host = Zen.Base.Host;

namespace Zen.Service.Maintenance.Service
{
    public class MaintenanceService : IHostedService, IDisposable
    {
        private static readonly List<Item> AllMaintenanceTasks;
        private static readonly List<Item> LocalInstanceMaintenanceTasks;
        private static readonly List<Item> DistributedMaintenanceTasks;

        private Timer _timer;

        static MaintenanceService()
        {
            // First collect all Maintenance tasks.
            var validTasks = IoC.GetClassesByInterface<IMaintenanceTask>();

            var preList = validTasks.Select(i =>
            {
                var item = new Item
                {
                    Type = i,
                    Instance = i.ToInstance<IMaintenanceTask>()
                };

                return item;
            }).ToList();

            foreach (var i in preList)
            {
                i.Setup = (MaintenanceTaskSetupAttribute)i.Type.GetMethod("MaintenanceTask")?.GetCustomAttributes(typeof(MaintenanceTaskSetupAttribute), false).FirstOrDefault()
                          ?? new MaintenanceTaskSetupAttribute { Name = i.Type.FullName + " Maintenance Task" };

                i.Priority = (PriorityAttribute)i.Type.GetMethod("MaintenanceTask")?.GetCustomAttributes(typeof(PriorityAttribute), false).FirstOrDefault()
                             ?? new PriorityAttribute { Level = 0 };

                i.Description = i.Setup?.Name;

                i.Id = $"{i.Type.FullName} | {i.Description}".ToGuid().ToString();
            }

            preList.Sort((a, b) => b.Priority.Level - a.Priority.Level);

            AllMaintenanceTasks = preList;

            //AllMaintenanceTasks =
            //    Resolution.GetClassesByInterface<IMaintenanceTask>()
            //        .Select(i =>
            //        {
            //            var item = new Item
            //            {
            //                Type = i,
            //                Instance = i.ToInstance<IMaintenanceTask>(),
            //                Setup = (MaintenanceTaskSetup)i.GetMethod("MaintenanceTask")?.GetCustomAttributes(typeof(MaintenanceTaskSetup), false).FirstOrDefault()
            //                        ?? new MaintenanceTaskSetup { Name = i.FullName + " Maintenance Task" },
            //                Priority = (PriorityAttribute)i.GetMethod("MaintenanceTask")?.GetCustomAttributes(typeof(PriorityAttribute), false).FirstOrDefault()
            //                           ?? new PriorityAttribute { Level = 0 }
            //            };

            //            item.Description = i.FullName + " | " + item.Setup.Name;
            //            item.Id = item.Description.StringToGuid().ToString();

            //            return item;
            //        })
            //        .OrderByDescending(i => i.Priority)
            //        .ToList();

            LocalInstanceMaintenanceTasks =
                AllMaintenanceTasks
                    .Where(i => i.Setup.LocalInstance)
                    .ToList();

            DistributedMaintenanceTasks =
                AllMaintenanceTasks
                    .Where(i => !i.Setup.LocalInstance)
                    .ToList();
        }

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
            public PriorityAttribute Priority { get; set; }

            #region Overrides of Object

            public override string ToString()
            {
                var sb = new StringBuilder();

                sb.Append($"[{Priority?.Level}]".PadLeft(5));
                sb.Append($" - {Setup?.Name}");
                sb.Append($", CD: {Setup?.Cooldown.ToString()}");

                if (Setup?.Orchestrated == true) sb.Append(", Orchestrated");

                return sb.ToString();
            }

            #endregion
        }

        #region Implementation of IHostedService

        public Task StartAsync(CancellationToken cancellationToken)
        {
            if (LocalInstanceMaintenanceTasks.Count == 0) return Task.CompletedTask;

            _timer = new Timer(RunMaintenance, null, TimeSpan.FromSeconds(5), TimeSpan.FromMinutes(5));

            return Task.CompletedTask;
        }

        private static readonly object LockObject = new object();

        public enum EScope
        {
            Distributed,
            LocalInstance
        }

        private void RunMaintenance(object state) { RunMaintenance(EScope.LocalInstance); }

        public static void RunMaintenance(EScope scope = EScope.Distributed)
        {
            if (!Monitor.TryEnter(LockObject)) return;

            var taskSet = scope == EScope.Distributed ? DistributedMaintenanceTasks : LocalInstanceMaintenanceTasks;

            var taskMap = taskSet.ToDictionary(i => i, i =>
            {
                var trackId = i.Id;
                if (i.Setup.LocalInstance) trackId = $"{Environment.MachineName.ToLower()}.{Host.ApplicationAssemblyName}.{trackId}";

                var trackItem = Tracking.Get(trackId) ?? new Tracking { Id = trackId, Description = i.Description };

                trackItem.InstanceIdentifier = Environment.MachineName;

                return trackItem;
            });

            try
            {
                var runnableTaskMap = taskMap.Where(i => i.Value.CanRun()).ToList();

                if (!runnableTaskMap.Any()) return;

                Log.Divider();
                Log.Maintenance<MaintenanceService>($"Running {taskSet.Count} {scope} maintenance task(s)");
                foreach (var (key, value) in runnableTaskMap) Log.KeyValuePair(key.Type.FullName, value.ToString(), Message.EContentType.Maintenance);
                Log.Divider();

                foreach (var (item, tracking) in runnableTaskMap)
                {
                    Task<Result> task = null;
                    var sw = new Stopwatch();
                    Ticket distributedTaskHandler = null;

                    sw.Start();

                    try
                    {

                        if (item.Setup.Orchestrated)
                        {
                            distributedTaskHandler = Factory.GetTicket(item.Description);
                            if (!distributedTaskHandler.CanRun()) continue;
                            distributedTaskHandler.Start();
                        }

                        Current.Log.KeyValuePair("Task", $"[{item.Id}] {item}", Message.EContentType.Maintenance);

                        task = item.Instance.MaintenanceTask();

                        task?.Wait();

                    }
                    catch (Exception e)
                    {
                        Current.Log.Add(e);

                        if (task != null)
                        {
                            task.Result.Status = Result.EResultStatus.Failed;
                            task.Result.Message = e.ToSummary();
                        }
                        else
                        {
                            tracking.LastMessage = e.ToSummary();
                            tracking.Success = false;
                        }
                    }
                    finally
                    {
                        sw.Stop();
                        distributedTaskHandler?.Stop();


                        if (task != null)
                        {
                            if (task.Exception != null)
                            {
                                task.Result.Status = Result.EResultStatus.Failed;
                                task.Result.Message = task.Exception.ToSummary();
                            }

                            task.Result.Counters?.ToLog();

                            tracking.LastResult = task.Result;
                            tracking.Success = task.Result.Status == Result.EResultStatus.Success;
                            tracking.Elapsed = sw.Elapsed.ToString();
                            tracking.LastMessage = tracking.LastResult?.Message;
                        }

                        tracking.LastRun = DateTime.Now;

                        if (item.Setup.Cooldown != default) tracking.NextRun = tracking.LastRun.Add(item.Setup.Cooldown);
                        tracking.RunOnce = item.Setup.RunOnce;

                        tracking.Save();

                    }
                }
            }
            finally
            {
                Monitor.Exit(LockObject);
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            Current.Log.Info("Orchestrated Background Maintenance is stopping.");
            _timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }

        #endregion
    }
}
