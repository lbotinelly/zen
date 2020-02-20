using System;
using System.ComponentModel.DataAnnotations;
using Zen.App.Core.Application;
using Zen.Base.Module;

namespace Zen.App.Model.Core {
    public class Permission : Data<Permission>, IPermission
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Code { get; set; }
        [Display]
        public string FullCode { get; set; }
        public string Name { get; set; }
        public string ApplicationId { get; set; }
    }
}