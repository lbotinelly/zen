using System;
using System.IO;
using System.Threading.Tasks;
using Zen.Base.Common;
using Zen.Base.Module.Log;
using Zen.Service.Maintenance.Attributes;
using Zen.Service.Maintenance.Model;
using Zen.Service.Maintenance.Shared;

namespace Zen.Storage.Cache {
    public class Maintenance : IMaintenanceTask
    {
        private const int CutoutDays = 1;

        #region Implementation of IMaintenanceTask

        [Priority(Level = -10), MaintenanceTaskSetup(Name = "Zen: Local Storage Cache maintenance", Schedule = "1:00:00", LocalInstance = true)]
        public Task<Result> MaintenanceTask()
        {
            var ret = new Result {Counters = new TagClicker(" Local Storage Maintenance - ")};
            var cutout = DateTime.Now.AddDays(CutoutDays * -1);

            if (!Directory.Exists(Local.BasePath)) return Task.FromResult(ret);

            foreach (var filePath in Directory.EnumerateFiles(Local.BasePath, "*", SearchOption.AllDirectories))
            {
                var probe = File.GetCreationTime(filePath);
                if (probe < cutout)
                {
                    File.Delete(filePath);
                    ret.Counters.Click("Removed");
                }
                else { ret.Counters.Click("Skipped"); }
            }

            return Task.FromResult(ret);
        }

        #endregion
    }
}