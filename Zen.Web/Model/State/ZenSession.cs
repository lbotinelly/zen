using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Zen.Base.Common;
using Zen.Base.Extension;
using Zen.Base.Module;
using Zen.Base.Module.Log;
using Zen.Service.Maintenance.Attributes;
using Zen.Service.Maintenance.Model;
using Zen.Service.Maintenance.Shared;

namespace Zen.Web.Model.State
{
    [Priority(Level = -99)]
    public class ZenSession : Data<ZenSession>, IZenSession, IMaintenanceTask
    {
        #region Implementation of IMaintenanceTask

        [MaintenanceTaskSetup(Name = "Framework: Session cleanup", Schedule = "1:00:00"), Priority(Level = 5)]
        public Task<Result> MaintenanceTask()
        {
            var result = new Result();

            var c = new TagClicker();

            // Return all sessions at least six hours old.

            var sessions = Where(i => i.LastUpdate <= DateTime.Now.AddHours(-6)).ToList();

            c.Click("Invalid sessions", sessions);

            Remove(sessions);

            result.Status = Result.EResultStatus.Success;
            result.Message = $"{sessions.Count} stale sessions removed";

            c.ToLog();

            return Task.FromResult(result);
        }

        #endregion

        [Key]
        public string Id { get; set; }
        public IDictionary<string, byte[]> Store { get; set; }
        public DateTime? Creation { get; set; } = DateTime.Now;
        public DateTime? LastUpdate { get; set; }

        public void Set<T>(string key, T data)
        {
            if (data == null)
                if (Store.ContainsKey(key))
                {
                    Store.Remove(key);
                    return;
                }

            Store[key] = data.ToByteArray();
        }

        public T Get<T>(string key) { return !Store.ContainsKey(key) ? default : Store[key].FromByteArray<T>(); }
    }
}