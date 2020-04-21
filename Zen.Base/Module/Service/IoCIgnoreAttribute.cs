using System;

namespace Zen.Base.Module.Service
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class IoCIgnoreAttribute : Attribute { }
}