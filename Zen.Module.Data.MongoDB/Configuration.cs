using Microsoft.Extensions.Options;
using Zen.Base.Common;
using Zen.Base.Module.Service;

namespace Zen.Module.Data.MongoDB
{
    public class Configuration : IConfigureOptions<Configuration.Options>
    {
        private readonly IOptions _options;

        public Configuration(IOptions<Options> options) => _options = options.Value;

        public void Configure(Options options)
        {
            options.ConnectionString = _options.ConnectionString;
        }

        public interface IOptions
        {
            string ConnectionString { get; }
        }

        [IoCIgnore]
        public class Options : IOptions
        {
            public string ConnectionString { get; set; }
        }

        [Priority(Level = -99)]
        public class AutoOptions : IOptions // If nothing else is defined, AutoOptions kicks in.
        {
            public string ConnectionString { get; } = @"mongodb://localhost:27017/default";
        }
    }
}
