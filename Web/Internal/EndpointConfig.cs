using Microsoft.Extensions.Configuration;

namespace Zen.Web.Internal {
    internal class EndpointConfig
    {
        public string Name { get; set; }
        public string Url { get; set; }
        public IConfigurationSection ConfigSection { get; set; }
    }
}