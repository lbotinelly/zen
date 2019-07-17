using System;

namespace Zen.App.Audience {
    public class PeriodDefinition
    {
        public DateTime? StartTime { get; set; } = DateTime.Now;
        public DateTime? EndTime { get; set; } = null;
    }
}