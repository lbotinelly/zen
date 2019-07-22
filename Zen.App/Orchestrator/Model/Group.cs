using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Zen.App.Provider;
using Zen.Base.Module;

namespace Zen.App.Orchestrator.Model
{
    public class Group : Data<Group>, IZenGroup
    {
        #region Implementation of IDataId

        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        #endregion

        #region Implementation of IDataCode

        [Display]
        public string Code { get; set; }

        #endregion
        public string OwnerAssetId { get; set; }
        public string Name { get; set; }
        public bool Active { get; set; }
        public string ParentId { get; set; }
        public List<Permission> Permissions { get; set; }


        public class Permission
        {
            public string Id { get; set; } = Guid.NewGuid().ToString();
            public string Code { get; set; }
            public string Name { get; set; }
        }

        #region Implementation of IDataActive
        public bool IsActive { get; set; } = true;
        #endregion

        #region Implementation of IZenGroup
        public bool FromSettings { get; set; }
        #endregion
    }
}