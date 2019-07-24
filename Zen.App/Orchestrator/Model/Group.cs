using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Zen.App.Provider;
using Zen.Base.Module;

namespace Zen.App.Orchestrator.Model
{
    public class Group : Data<Group>, IZenGroup
    {
        public string OwnerAssetId { get; set; }
        public string Name { get; set; }
        public string ParentId { get; set; }

        #region Implementation of IDataActive

        public bool IsActive { get; set; } = true;

        #endregion

        #region Implementation of IDataId

        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        #endregion

        #region Implementation of IDataCode

        [Display]
        public string Code { get; set; }

        #endregion

        public bool Active { get; set; }

        #region Implementation of IZenGroup

        public List<Application.Permission> Permissions { get; set; }
        public bool FromSettings { get; set; }

        #endregion
    }
}