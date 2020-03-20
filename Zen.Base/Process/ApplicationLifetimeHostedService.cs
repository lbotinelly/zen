using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Zen.Base.Module.Log;

namespace Zen.Base.Process
{
    // https://dejanstojanovic.net/aspnet/2018/june/clean-service-stop-on-linux-with-net-core-21/
    public class ApplicationLifetimeHostedService : IHostedService
    {
        private readonly IHostApplicationLifetime appLifetime;
        private IConfiguration configuration;
        private IHostEnvironment environment;

        public ApplicationLifetimeHostedService(IConfiguration configuration, IHostEnvironment environment, IHostApplicationLifetime appLifetime)
        {
            this.configuration = configuration;
            this.appLifetime = appLifetime;
            this.environment = environment;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            Log.KeyValuePair(Host.ApplicationAssemblyName, "StartAsync", Message.EContentType.StartupSequence);

            appLifetime.ApplicationStarted.Register(OnStarted);
            appLifetime.ApplicationStopping.Register(OnStopping);
            appLifetime.ApplicationStopped.Register(OnStopped);

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            Log.KeyValuePair(Host.ApplicationAssemblyName, "StopAsync", Message.EContentType.StartupSequence);
            return Task.CompletedTask;
        }

        private void OnStarted() { Log.KeyValuePair(Host.ApplicationAssemblyName, "OnStarted", Message.EContentType.StartupSequence); }

        private void OnStopping()
        {
            Log.KeyValuePair(Host.ApplicationAssemblyName, "OnStopping", Message.EContentType.ShutdownSequence);
            appLifetime?.StopApplication();
        }

        private void OnStopped() { Log.KeyValuePair(Host.ApplicationAssemblyName, "OnStopped", Message.EContentType.ShutdownSequence); }
    }
}
