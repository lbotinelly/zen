using System.Collections.Generic;

namespace Zen.Web.Model.Configuration
{
    public class WebConfiguration
    {
        public int? HttpPort { get; set; } = 5000;
        public int? HttpsPort { get; set; } = 5001;
        public BehaviorDescriptor Behavior { get; set; }
        public EnvironmentDescriptor Development { get; set; }
        public EnvironmentDescriptor Production { get; set; }
        public string RoutePrefix {get; set; }

        public class BehaviorDescriptor
        {
            public bool UseAppCodeAsRoutePrefix { get; set; } = false;
        }

        public string DevelopmentCertificateSubject { get; set; }
        public string DevelopmentQualifiedServerName { get; set; }

        public class EnvironmentDescriptor
        {
            public BehaviorDescriptor Behavior { get; set; }
            public string CertificateSubject { get; set; }
            public string QualifiedServerName { get; set; }
            public int? HttpPort { get; set; } = 5000;
            public int? HttpsPort { get; set; } = 5001;
        }
    }
}