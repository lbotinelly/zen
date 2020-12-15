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

        public SelfHostOrchestrator(IOptions<Configuration.Options> options) : this(options.Value) { }

        public SelfHostOrchestrator(Configuration.Options options) => Options = options;

        #region Implementation of IZenProvider

        public EOperationalStatus OperationalStatus { get; }

        public void Initialize() { }

        public string GetState() => OperationalStatus.ToString();

        public async Task Start()
        {
            Options.Evaluate();
            await SetUpnp();
            await SetCertificate();
        }

        private async Task SetCertificate()
        {
            try
            {

            }
            catch (Exception e)
            {
                Log.Add(e);
                throw;
            }
        }

        public async Task SetUpnp()
        {
            try
            {
                var discoverer = new NatDiscoverer();
                var cts = new CancellationTokenSource(Options.DiscoveryTimeOut);
                var device = await discoverer.DiscoverDeviceAsync(PortMapper.Upnp, cts);

                var wanIp = await device.GetExternalIPAsync();

                await device.CreatePortMapAsync(new Mapping(Protocol.Tcp, Options.WanHttpPort, Options.LanHttpPort, Options.HttpMappingAlias));
                await device.CreatePortMapAsync(new Mapping(Protocol.Tcp, Options.WanHttpsPort, Options.LanHttpsPort, Options.HttpsMappingAlias));

                Log.Startup<SelfHostOrchestrator>($"WAN {wanIp}:{Options.WanHttpPort} => {Options.LanHttpPort} | {Options.HttpMappingAlias}");
                Log.Startup<SelfHostOrchestrator>($"WAN {wanIp}:{Options.WanHttpsPort} => {Options.LanHttpsPort} | {Options.HttpsMappingAlias}");
            }
            catch (Exception e)
            {
                Log.Add(e);
                throw;
            }
        }

        #endregion
    }
}