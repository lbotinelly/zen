using System.Collections.Generic;

namespace Zen.App.Model.Configuration
{
    public class ApplicationConfiguration
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Locator { get; set; }
        public List<GroupDescriptor> Groups { get; set; }
        public string FunctionalArea { get; set; }

        public class GroupDescriptor
        {
            public string Name { get; set; }
            public List<string> Permissions { get; set; }
            public List<string> Members { get; set; }
            public bool IsHost { get; set; } = false;
            public string Code { get; set; }
        }
    }
}