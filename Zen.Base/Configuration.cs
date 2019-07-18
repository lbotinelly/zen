using System.IO;
using Microsoft.Extensions.Configuration;

namespace Zen.Base
{
    public static class Configuration
    {
        static Configuration()
        {
            var configurationBuilder =
                    new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("zen.json", true, true)
                    .AddEnvironmentVariables()
                ;

            Options = configurationBuilder.Build();
        }

        public static IConfigurationRoot Options { get; }
    }
}