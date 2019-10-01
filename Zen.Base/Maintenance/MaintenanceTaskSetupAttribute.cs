using System;

namespace Zen.Base.Maintenance {
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class MaintenanceTaskSetupAttribute : Attribute
    {
        public string Name;
        public TimeSpan Cooldown = TimeSpan.FromMinutes(30);  // Default behavior: run every 30 mins.
        public bool RunOnce = false;
        public string Schedule
        {
            get => Cooldown.ToString();
            set => Cooldown = TimeSpan.Parse(value);
        }
        public bool Orchestrated { get; set; } = false;
    }
}