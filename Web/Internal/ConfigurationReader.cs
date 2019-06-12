using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;

namespace Zen.Web.Internal
{
    internal class ConfigurationReader
    {
        private const string ProtocolsKey = "Protocols";
        private const string EndpointDefaultsKey = "EndpointDefaults";

        private readonly IConfiguration _configuration;
        private EndpointDefaults _endpointDefaults;
        private IList<EndpointConfig> _endpoints;

        public ConfigurationReader(IConfiguration configuration) { _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration)); }

        public EndpointDefaults EndpointDefaults
        {
            get
            {
                if (_endpointDefaults == null) ReadEndpointDefaults();

                return _endpointDefaults;
            }
        }

        private void ReadEndpointDefaults()
        {
            var configSection = _configuration.GetSection(EndpointDefaultsKey);

            _endpointDefaults = new EndpointDefaults
            {
                //Protocols = ParseProtocols(configSection[ProtocolsKey])
            };
        }

    }
}