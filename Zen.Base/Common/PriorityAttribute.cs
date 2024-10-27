using System;

namespace Zen.Base.Common
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public sealed class PriorityAttribute : Attribute
    {
        public int Level { get; set; } = 0;
    }
}