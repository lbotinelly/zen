using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Zen.App.Provider;
using Zen.Base.Module;

namespace Zen.App.Orchestrator.Model
{
    public class Application : Data<Application>, IZenApplication
    {
        public bool Locked { get; set; }
        public string Description { get; set; }
        public ConfigurationBlock Configuration { get; set; }

        #region Implementation of IDataId

        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        #endregion

        #region Implementation of IDataLocator

        [Display]
        public string Locator { get; set; }

        #endregion

        #region Implementation of IDataCode

        public string Code { get; set; }

        #endregion

        public bool Active { get; set; } = true;
        public string Name { get; set; }
        public List<Permission> Permissions { get; set; }

        public override void BeforeUpdate()
        {
            foreach (var p in Permissions) p.FullCode = $"[{Code}].[{p.Code}]";
        }

        public class ConfigurationBlock
        {
            public string Url { get; set; }  
            public string VersionTag { get; set; }
            public bool IsLegacy { get; set; }
        }

        public class Permission : Data<Permission>, IZenPermission
        {
            [Key]
            public string Id { get; set; } = Guid.NewGuid().ToString();
            public string Code { get; set; }
            [Display]
            public string FullCode { get; set; }
            public string Name { get; set; }
            public string ApplicationId { get; set; }
        }

        public class Settings
        {
            public class Group
            {
                public string Code { get; set; }
                public string Name { get; set; }
                public List<string> Permissions { get; set; }
                public List<string> Members { get; set; }
                public bool IsHost { get; set; } = false;
                public bool Auto { get; set; } = false;
            }

            public string Code { get; set; }
            public string Name { get; set; }
            public string Locator { get; set; }
            public List<Group> Groups { get; set; }
        }
    }
}