using Zen.Web.Data.Controller.Attributes;

namespace Zen.Web.Data.Controller
{
    public class EndpointConfiguration
    {
        public DataBehavior Behavior = new DataBehavior();
        public DataSecurity Security = new DataSecurity();
    }
}