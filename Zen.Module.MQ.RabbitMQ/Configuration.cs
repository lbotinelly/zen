using Microsoft.Extensions.Options;
using Zen.Base.Common;
using Zen.Base.Module.Service;

namespace Zen.Module.MQ.RabbitMQ
{
    public class Configuration : IConfigureOptions<Configuration.Options>
    {
        private readonly IOptions _options;

        public Configuration(IOptions<Options> options) => _options = options.Value;

        public void Configure(Options options)
        {
            options.HostName = _options.HostName;
        }

        public interface IOptions
        {
            string HostName { get; set; }
            bool Durable { get; set; }
            bool Exclusive { get; set; }
            bool AutoDelete { get; set; }
        }

        [IoCIgnore]
        public class Options : IOptions
        {
            public string HostName { get; set; }
            public bool Durable { get; set; }
            public bool Exclusive { get; set; }
            public bool AutoDelete { get; set; }
        }

        [Priority(Level = -99)]
        public class AutoOptions : IOptions // If nothing else is defined, AutoOptions kicks in.
        {
            public string HostName { get; set; } = @"localhost";
            public bool Durable { get; set; } = true;
            public bool Exclusive { get; set; } = false;
            public bool AutoDelete { get; set; } = true;
        }
    }
}
