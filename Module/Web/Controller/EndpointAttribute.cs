using System;

namespace Zen.Module.Web.Controller
{
    public class EndpointConfiguration
    {
        public BehaviorAttribute Behavior = new BehaviorAttribute();

        public SecurityAttribute Security = new SecurityAttribute();

        [AttributeUsage(AttributeTargets.Class)]
        public class SecurityAttribute : Attribute
        {
            public string Read { get; set; }
            public string Write { get; set; }
            public string Remove { get; set; }
        }

        [AttributeUsage(AttributeTargets.Class)]
        public class BehaviorAttribute : Attribute
        {
            public Type SummaryType { get; set; }
            public bool MustPaginate { get; set; }
            public bool CacheResults { get; set; }
        }
    }
}