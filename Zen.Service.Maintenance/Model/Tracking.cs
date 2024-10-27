using System;
using System.ComponentModel.DataAnnotations;
using Zen.Base.Module;
using Zen.Base.Module.Data;
using Zen.Base.Module.Data.CommonAttributes;

namespace Zen.Service.Maintenance.Model
{
    [DataConfig(UseCaching = false)]
    public class Tracking : Data<Tracking>, IDataId
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        [Display]
        public string Description { get; set; }
        public Result LastResult { get; set; }
        public DateTime NextRun { get; set; }
        public DateTime LastRun { get; set; } = DateTime.MinValue;
        public bool Success { get; set; }
        public bool RunOnce { get; set; }
        public string LastMessage { get; set; }
        public string Elapsed { get; set; }
        public string InstanceIdentifier { get; set; }

        public bool CanRun()
        {
            if (RunOnce && LastRun != DateTime.MinValue) return false;
            if (!Success) return true;
            return NextRun < DateTime.Now;
        }

        #region Overrides of Data<Tracking>

        public override string ToString()
        {
            var runMsg = CanRun() ? "scheduled" : "skipped";
            return $"{Description} ({runMsg})";
        }

        #endregion
    }
}