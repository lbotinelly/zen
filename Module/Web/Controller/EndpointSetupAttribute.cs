using System;

namespace Zen.Module.Web.Controller
{
    [AttributeUsage(AttributeTargets.Class)]
    public class EndpointSetupAttribute : System.Attribute
    {
        public BehaviorDefinition Behavior = new BehaviorDefinition();

        public SecurityDefinition Security = new SecurityDefinition();

        public class SecurityDefinition
        {
            public string Read { get; set; }
            public string Write { get; set; }
            public string Remove { get; set; }
        }

        public class BehaviorDefinition
        {
            public Type SummaryType { get; set; }
            public bool MustPaginate { get; set; }
            public bool CacheResults { get; set; }
        }
    }
}