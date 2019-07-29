using System;
using System.ComponentModel.DataAnnotations;
using Zen.App.Provider;
using Zen.Base.Module;

namespace Zen.App.Orchestrator.Model
{
    public partial class Application
    {
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
    }
}