using System;
using System.Collections.Generic;
using System.Text;

namespace Zen.Base.Module.Data.Pipeline.SetVersion.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class SetVersionPermissionAttribute : Attribute
    {
        public string Modify { get; set; } = null;
        public string Read { get; set; } = null;
        public string Browse { get; set; } = null;
    }
}
