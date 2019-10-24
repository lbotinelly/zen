using System;

namespace Zen.Base.Module.Data
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public sealed class DataEnvironmentMappingAttribute : Attribute
    {
        public string Origin { get; set; }
        public string Target { get; set; }
    }
}