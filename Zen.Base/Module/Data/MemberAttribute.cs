using System;

namespace Zen.Base.Module.Data
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = true)]
    public class MemberAttribute : Attribute
    {
        public string Name { get; set; }
        public EMemberType Interface { get; set; } = EMemberType.Undefined;
        public Type Type { get; set; }
        public int? Size { get; set; }

        public override string ToString() => $"'{Name}' ({Interface}, {Type.Name})";
    }

    public enum EMemberType {
        Property,
        Field,
        Undefined
    }
}