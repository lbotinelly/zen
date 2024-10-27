using Microsoft.Extensions.Options;
using Zen.Base.Common;
using Zen.Base.Module.Service;

namespace Zen.Module.Data.SqlServer
{
    public class Configuration : IConfigureOptions<Configuration.Options>
    {
        private readonly IOptions _options;

        public Configuration(IOptions<Options> options) => _options = options.Value;

        public void Configure(Options options)
        {
            options.AdministrativeConnectionString = _options.AdministrativeConnectionString;
            options.ConnectionString = _options.ConnectionString;
        }

        public interface IOptions
        {
            string AdministrativeConnectionString { get; }
            string ConnectionString { get; }
            bool AttempSchemaSetup { get; set; }
        }

        [IoCIgnore]
        public class Options : IOptions
        {
            public string AdministrativeConnectionString { get; set; }
            public string ConnectionString { get; set; }
            public bool AttempSchemaSetup { get; set; }
        }

        [Priority(Level = -99)]
        public class AutoOptions : IOptions // If nothing else is defined, AutoOptions kicks in.
        {
            public string AdministrativeConnectionString { get; } = @"Data Source=localhost;persist security info=True;Integrated Security = SSPI;";
            public string ConnectionString { get; } = @"Data Source=localhost;persist security info=True;Integrated Security = SSPI;";
            public bool AttempSchemaSetup { get; set; } = true;
        }
    }
}