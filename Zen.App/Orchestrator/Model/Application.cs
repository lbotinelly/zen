using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Zen.App.Provider;
using Zen.App.Provider.Application;
using Zen.Base.Module;

namespace Zen.App.Orchestrator.Model
{
    public partial class Application : Data<Application>, IZenApplication
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
        public virtual List<IZenPermission> GetPermissions() { return Permission.Where(i => i.ApplicationId == Id).Select(i => (IZenPermission)i).ToList(); }

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
    }
}