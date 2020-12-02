using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Open.Nat;
using Zen.Base;
using Zen.Base.Common;
using Zen.Web.SelfHost.Common;

namespace Zen.Web.SelfHost
{
    public class SelfHostOrchestrator : ISelfHostOrchestrator
    {
        public readonly Configuration.Options Options;

        public SelfHostOrchestrator(IOptions<Configuration.Options> options) : this(options.Value)
        {
        }

        public SelfHostOrchestrator(Configuration.Options options)
        {
            options.Evaluate();
            Options = options;
        }

        #region Implementation of IZenProvider

        public EOperationalStatus OperationalStatus { get; }


        public void Initialize()
        {
            // throw new NotImplementedException();
        }

        public string GetState()
        {
            return OperationalStatus.ToString();
        }

        public async Task Start()
        {
            try
            {
                var discoverer = new NatDiscoverer();
                var cts = new CancellationTokenSource(Options.DiscoveryTimeOut);
                var device = await discoverer.DiscoverDeviceAsync(PortMapper.Upnp, cts);

                var WanIp = await device.GetExternalIPAsync();

                await device.CreatePortMapAsync(new Mapping(Protocol.Tcp, Options.WanHttpPort, Options.LanHttpPort,
                    Options.HttpMappingAlias));
                await device.CreatePortMapAsync(new Mapping(Protocol.Tcp, Options.WanHttpsPort, Options.LanHttpsPort,
                    Options.HttpsMappingAlias));

                Log.Startup<SelfHostOrchestrator>($"WAN {WanIp}:{Options.WanHttpPort} => {Options.LanHttpPort} | {Options.HttpMappingAlias}");
                Log.Startup<SelfHostOrchestrator>($"WAN {WanIp}:{Options.WanHttpsPort} => {Options.LanHttpsPort} | {Options.HttpsMappingAlias}");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        #endregion
    }
}