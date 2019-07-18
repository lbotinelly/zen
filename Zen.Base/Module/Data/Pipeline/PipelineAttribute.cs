using System;

namespace Zen.Base.Module.Data.Pipeline
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public sealed class PipelineAttribute : Attribute
    {
        public Type[] Types;
        public PipelineAttribute(params Type[] types) { Types = types; }
    }
}