using System;

namespace Zen.Base.Maintenance
{
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class MaintenanceTaskSetup : Attribute
    {
        public TimeSpan Cooldown = TimeSpan.FromMinutes(30); // Default behavior: run every 30 mins.
        public string Name;
        public string Schedule { get => Cooldown.ToString(); set => Cooldown = TimeSpan.Parse(value); }
        public bool RunOnce = false;
        public bool Orchestrated = false;
        public bool LocalInstance = false;
    }
}