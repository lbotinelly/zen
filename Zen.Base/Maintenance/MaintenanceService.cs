using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Zen.Base.Common;
using Zen.Base.Distributed;
using Zen.Base.Extension;
using Zen.Base.Module.Log;
using Zen.Base.Module.Service;

namespace Zen.Base.Maintenance
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
            var validTasks = Resolution.GetClassesByInterface<IMaintenanceTask>();

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
                i.Setup = (MaintenanceTaskSetup) i.Type.GetMethod("MaintenanceTask")?.GetCustomAttributes(typeof(MaintenanceTaskSetup), false).FirstOrDefault()
                          ?? new MaintenanceTaskSetup {Name = i.Type.FullName + " Maintenance Task"};

                i.Priority = (PriorityAttribute) i.Type.GetMethod("MaintenanceTask")?.GetCustomAttributes(typeof(PriorityAttribute), false).FirstOrDefault()
                             ?? new PriorityAttribute {Level = 0};

                i.Description = i.Setup?.Name;

                i.Id = $"{i.Type.FullName} | {i.Description}".StringToGuid().ToString();
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
            internal MaintenanceTaskSetup Setup { get; set; }
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

        private void RunMaintenance(object state) { RunMaintenance(state, EScope.LocalInstance); }

        public static void RunMaintenance(object state = null, EScope scope = EScope.Distributed)
        {
            if (!Monitor.TryEnter(LockObject)) return;

            var taskSet = scope == EScope.Distributed ? DistributedMaintenanceTasks : LocalInstanceMaintenanceTasks;

            var taskMap = taskSet.ToDictionary(i => i, i =>
            {
                var trackId = i.Id;
                if (i.Setup.LocalInstance) trackId = $"{Environment.MachineName.ToLower()}.{Host.ApplicationAssemblyName}.{trackId}";

                var trackItem = Tracking.Get(trackId) ?? new Tracking {Id = trackId, Description = i.Description};

                trackItem.InstanceIdentifier = Environment.MachineName;

                return trackItem;
            });

            try
            {
                var runnableTaskMap = taskMap.Where(i => i.Value.CanRun()).ToList();

                if (!runnableTaskMap.Any()) return;

                Log.Divider();
                Log.Maintenance<MaintenanceService>($"Running {taskSet.Count} {scope} maintenance task(s)");
                foreach (var item in runnableTaskMap) Log.KeyValuePair(item.Key.Type.FullName, item.Value.ToString(), Message.EContentType.Maintenance);
                Log.Divider();

                foreach (var item in runnableTaskMap)
                {
                    var ms = item.Key;
                    var tk = item.Value;
                    Task<Result> task = null;
                    var sw = new Stopwatch();
                    Ticket distributedTaskHandler = null;

                    sw.Start();

                    try
                    {

                        if (ms.Setup.Orchestrated)
                        {
                            distributedTaskHandler = Factory.GetTicket(ms.Description);
                            if (!distributedTaskHandler.CanRun()) continue;
                            distributedTaskHandler.Start();
                        }

                        Current.Log.KeyValuePair("Task", $"[{ms.Id}] {item}", Message.EContentType.Maintenance);

                        task = ms.Instance.MaintenanceTask();

                        task?.Wait();

                    } catch (Exception e)
                    {
                        if (task != null)
                        {
                            task.Result.Status = Result.EResultStatus.Failed;
                            task.Result.Message = e.ToSummary();
                        }
                        else
                        {
                            tk.LastMessage = e.ToSummary();
                            tk.Success = false;
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

                            tk.LastResult = task.Result;
                            tk.Success = task.Result.Status == Result.EResultStatus.Success;
                            tk.Elapsed = sw.Elapsed;
                            tk.LastMessage = tk.LastResult?.Message;
                        }

                        tk.LastRun = DateTime.Now;

                        if (ms.Setup.Cooldown != default) tk.NextRun = tk.LastRun.Add(ms.Setup.Cooldown);
                        tk.RunOnce = ms.Setup.RunOnce;

                        tk.Save();

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
            Current.Log.Info("Orchestrasted Background Maintenance is stopping.");

            _timer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }

        #endregion
    }
}