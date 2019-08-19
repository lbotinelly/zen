using System.Collections.Generic;

namespace Zen.Web.Model.Configuration
{
    public class WebConfiguration
    {
        public BehaviorDescriptor Behavior { get; set; }

        public class BehaviorDescriptor
        {
            public bool UseAppCodeAsRoutePrefix { get; set; } = false;
        }
    }
}