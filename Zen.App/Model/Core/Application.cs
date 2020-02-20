using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Zen.App.Core.Application;
using Zen.App.Core.Group;
using Zen.Base.Module;

namespace Zen.App.Model.Core
{
    public class Application : Data<Application>, IApplication
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
        public virtual List<IPermission> GetPermissions() { return Permission.Where(i => i.ApplicationId == Id).Select(i => (IPermission) i).ToList(); }

        public List<IGroup> GetGroups() { return Group.Where(i => i.ApplicationId == Id).Select(i => (IGroup) i).ToList(); }

        public IGroup GetGroup(string code) { return GetGroups().FirstOrDefault(i => i.Code.EndsWith("_" + code)); }

        public override void BeforeUpdate()
        {
            var permissions = GetPermissions();

            foreach (var p in permissions)
            {
                var targetCode = $"[{Code}].[{p.Code}]";

                if (p.FullCode == targetCode) continue;

                p.FullCode = targetCode;
                ((Permission) p).Save();
            }
        }
    }
}