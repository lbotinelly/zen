using Microsoft.Extensions.Options;
using Zen.Base.Common;
using Zen.Base.Extension;
using Zen.Base.Module.Service;

namespace Zen.Web
{
    public class Configuration : IConfigureOptions<Configuration.Options>
    {
        public enum EMode
        {
            Auto,
            Custom
        }

        private readonly IOptions _options;

        public Configuration(IOptions<Options> options) => _options = options.Value;

        public void Configure(Options options)
        {
            _options.CopyMembersTo(options);
        }

        public interface IOptions
        {
            string WebQualifiedServerName { get; set; }
            AutoOptions.EnvironmentDescriptor Development { get; set; }
            AutoOptions.EnvironmentDescriptor Production { get; set; }
            AutoOptions.EnvironmentDescriptor GetCurrentEnvironment();
        }

        [IoCIgnore]
        public class Options : AutoOptions { }

        [Priority(Level = -99)]
        public class AutoOptions : IOptions // If nothing else is defined, AutoOptions kicks in.
        {
            public static int DefaultHttpPort = 5000;
            public static int DefaultHttpsPort = 5001;
            public string WebQualifiedServerName { get; set; } = null;

            public EnvironmentDescriptor Development { get; set; } = new EnvironmentDescriptor();
            public EnvironmentDescriptor Production { get; set; }= new EnvironmentDescriptor();

            public EnvironmentDescriptor GetCurrentEnvironment() => Base.Host.IsDevelopment ? Development : Production;

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
}