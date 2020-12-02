namespace Zen.Web.Configuration
{
    public class Options
    {
        public static int DefaultHttpPort = 5000;
        public static int DefaultHttpsPort = 5001;
        public string WebQualifiedServerName { get; set; } = null;

        public EnvironmentDescriptor Development { get; set; }
        public EnvironmentDescriptor Production { get; set; }

        public EnvironmentDescriptor GetCurrentEnvironment()
        {
            return Base.Host.IsDevelopment ? Development : Production;
        }

        public class EnvironmentDescriptor
        {
            public BehaviorDescriptor Behavior { get; set; }
            public string CertificateSubject { get; set; }
            public string QualifiedServerName { get; set; }
            public int HttpPort { get; set; } = DefaultHttpPort;
            public int HttpsPort { get; set; } = DefaultHttpsPort;
            public string RoutePrefix { get; set; }

            public class BehaviorDescriptor
            {
                public bool UseAppCodeAsRoutePrefix { get; set; } = false;
            }
        }
    }
}