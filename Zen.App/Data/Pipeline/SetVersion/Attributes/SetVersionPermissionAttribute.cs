using System;

namespace Zen.App.Data.Pipeline.SetVersion.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class SetVersionPermissionAttribute : Attribute
    {
        public string Modify { get; set; } = null;
        public string Read { get; set; } = null;
        public string Browse { get; set; } = null;
    }
}
