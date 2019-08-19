using System.Collections.Generic;

namespace Zen.App.Orchestrator.Model
{
    public partial class Application
    {
        public class SettingsDescriptor
        {
            public class GroupDescriptor
            {
                public string Code { get; set; }
                public string Name { get; set; }
                public List<string> Permissions { get; set; }
                public List<string> Members { get; set; }
                public bool IsHost { get; set; } = false;
            }

            public string Code { get; set; }
            public string Name { get; set; }
            public string Locator { get; set; }
            public List<GroupDescriptor> Groups { get; set; }
        }
    }
}