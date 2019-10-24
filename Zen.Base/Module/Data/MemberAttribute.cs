using System;

namespace Zen.Base.Module.Data {
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = true)]
    public class MemberAttribute : Attribute
    {
        public string Name { get; set; }
    }
}