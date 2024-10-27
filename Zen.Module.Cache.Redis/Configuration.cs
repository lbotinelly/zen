using System.Collections.Generic;
using Microsoft.Extensions.Options;
using Zen.Base.Common;
using Zen.Base.Extension;
using Zen.Base.Module.Service;

namespace Zen.Module.Cache.Redis
{
    public class Configuration : IConfigureOptions<Configuration.Options>
    {
        private readonly IOptions _options;
        public Configuration(IOptions<Options> options) => _options = options.Value;
        public void Configure(Options options) => options.CopyProperties(_options);


        // Individual Configuration payload
        public class ConnectionConfiguration
        {
            public string ConnectionString { get; set; }
            public int DatabaseIndex { get; set; }
        }

        public interface IOptions
        {
            Dictionary<string, ConnectionConfiguration> EnvironmentSettings { get; set; }
        }

        [IoCIgnore]
        public class Options : IOptions
        {
            public Dictionary<string, ConnectionConfiguration> EnvironmentSettings { get; set; }
        }

        [Priority(Level = -99)]
        public class AutoOptions : IOptions // If nothing else is defined, AutoOptions kicks in.
        {
            public AutoOptions() =>
                EnvironmentSettings = new Dictionary<string, ConnectionConfiguration>
                {
                    {"STA", new ConnectionConfiguration {DatabaseIndex = 5, ConnectionString = "localhost"}}
                };

            public Dictionary<string, ConnectionConfiguration> EnvironmentSettings { get; set; }
        }
    }
}