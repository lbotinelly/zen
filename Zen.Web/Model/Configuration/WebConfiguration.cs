using Zen.Web.Host;

namespace Zen.Web.Model.Configuration
{
    public class WebConfiguration : EnvironmentDescriptor
    {
        public EnvironmentDescriptor Development { get; set; }
        public EnvironmentDescriptor Production { get; set; }

        public EnvironmentDescriptor GetCurrentEnvironment()
        {
            return Base.Host.IsDevelopment ? Development : Production;
        }
    }


    public class EnvironmentDescriptor
    {
        public BehaviorDescriptor Behavior { get; set; }
        public string CertificateSubject { get; set; }
        public string QualifiedServerName { get; set; }
        public int? HttpPort { get; set; } = Defaults.WebHttpPort;
        public int? HttpsPort { get; set; } = Defaults.WebHttpsPort;
        public string RoutePrefix { get; set; }

        public class BehaviorDescriptor
        {
            public bool UseAppCodeAsRoutePrefix { get; set; } = false;
        }
    }
}