using Microsoft.Extensions.Options;
using Zen.Base.Common;
using Zen.Base.Extension;
using Zen.Base.Module.Service;

namespace Zen.Web.SelfHost
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
            EMode Mode { get; set; }
            int DiscoveryTimeOut { get; set; }
            int WanHttpsPort { get; set; }
            int WanHttpPort { get; set; }
            string LocalCertificateStoragePassword { get; set; }
            void Evaluate();
        }

        [IoCIgnore]
        public class Options : AutoOptions { }

        [Priority(Level = -99)]
        public class AutoOptions : IOptions // If nothing else is defined, AutoOptions kicks in.
        {
            #region Implementation of IOptions

            public EMode Mode { get; set; } = EMode.Auto;
            public int DiscoveryTimeOut { get; set; } = 10000;
            public int WanHttpsPort { get; set; }
            public int WanHttpPort { get; set; }
            public string LocalCertificateStoragePassword { get; set; } = "zen_storage";

            public void Evaluate()
            {
                if (Mode != EMode.Auto) return;
                //LanHttpPort = Instances.Options.GetCurrentEnvironment().HttpPort;
                //LanHttpsPort = Instances.Options.GetCurrentEnvironment().HttpsPort;

                //WanHttpPort = 80;
                //WanHttpsPort = 443;

                var currEnv = Web.Current.ZenWebOrchestrator.Options.GetCurrentEnvironment();

                WanHttpPort = currEnv.HttpPort;
                WanHttpsPort = currEnv.HttpsPort;
                LanHttpPort = WanHttpPort;
                LanHttpsPort = WanHttpsPort;

                HttpMappingAlias = $"{App.Current.Orchestrator.Application.Name} HTTP";
                HttpsMappingAlias = $"{App.Current.Orchestrator.Application.Name} HTTPS";
            }

            public string HttpsMappingAlias { get; set; }

            public string HttpMappingAlias { get; set; }

            public int LanHttpsPort { get; set; }

            public int LanHttpPort { get; set; }

            #endregion
        }
    }
}