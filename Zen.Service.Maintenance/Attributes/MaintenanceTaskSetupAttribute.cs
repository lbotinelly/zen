using System;

namespace Zen.Service.Maintenance.Attributes
{
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class MaintenanceTaskSetupAttribute : Attribute
    {
        public TimeSpan Cooldown = TimeSpan.FromMinutes(30); // Default behavior: run every 30 mins.
        public bool LocalInstance = false;
        public string Name;
        public bool Orchestrated = false;
        public bool RunOnce = false;

        public string Schedule
        {
            get => Cooldown.ToString();
            set => Cooldown = TimeSpan.Parse(value);
        }
    }
}