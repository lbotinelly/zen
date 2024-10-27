using System;
using System.ComponentModel.DataAnnotations;
using Zen.App.Model.Audience;
using Zen.Base.Module;

namespace Zen.App.Model.Communication
{
    public abstract class AppScopedContent<T> : Data<T>, IAppScopedContent where T : Data<T>
    {
        //[Key]
        //internal abstract string Id { get; set; }
        public DateTime CreationTime { get; set; } = DateTime.Now;
        public string Author { get; set; } = Current.Orchestrator.Person?.Locator;
        public AudienceDefinition Audience { get; set; } = new AudienceDefinition();
        public string ApplicationCode { get; set; } = Current.Orchestrator.Application?.Code;
        public bool Active { get; set; } = true;
    }
}