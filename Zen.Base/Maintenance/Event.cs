using System;
using System.ComponentModel.DataAnnotations;
using Zen.Base.Module;
using Zen.Base.Module.Data;

namespace Zen.Base.Maintenance {
    [DataConfig(UseCaching = false)]
    public class Tracking : Data<Tracking>
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        [Display]
        public string Description { get; set; }
        public Result LastResult { get; set; }
        public DateTime NextRun { get; set; }
        public DateTime LastRun { get; set; } = DateTime.Now;
        public bool Success { get; set; }
        public string LastMessage { get; set; }
        public TimeSpan Elapsed { get; set; }
    }
}