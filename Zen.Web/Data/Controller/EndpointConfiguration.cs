using Zen.Web.Data.Controller.Attributes;

namespace Zen.Web.Data.Controller
{
    public class EndpointConfiguration
    {
        public DataBehaviorAttribute Behavior = new DataBehaviorAttribute();
        public DataSecurityAttribute Security = new DataSecurityAttribute();
    }
}