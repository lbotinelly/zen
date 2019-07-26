using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
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
        public List<IZenPermission> GetPermissions() { return Permission.Where(i => i.ApplicationId == Id).Select(i => (IZenPermission)i).ToList(); }

        public override void BeforeUpdate()
        {
            var permissions = GetPermissions();

            foreach (var p in permissions)
            {
                var targetCode = $"[{Code}].[{p.Code}]";

                if (p.FullCode == targetCode) continue;

                p.FullCode = targetCode;
                ((Permission)p).Save();
            }
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