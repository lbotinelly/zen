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
        private readonly IHostApplicationLifetime _appLifetime;
        private IConfiguration _configuration;
        private IHostEnvironment _environment;

        public ApplicationLifetimeHostedService(IConfiguration configuration, IHostEnvironment environment, IHostApplicationLifetime appLifetime)
        {
            this._configuration = configuration;
            this._appLifetime = appLifetime;
            this._environment = environment;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            Log.KeyValuePair(Host.ApplicationAssemblyName, "Started!", Message.EContentType.StartupSequence);

            _appLifetime.ApplicationStarted.Register(OnStarted);
            _appLifetime.ApplicationStopping.Register(OnStopping);
            _appLifetime.ApplicationStopped.Register(OnStopped);

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            Log.KeyValuePair(Host.ApplicationAssemblyName, "Stopped!", Message.EContentType.ShutdownSequence);
            return Task.CompletedTask;
        }


        private void OnStopping()
        {
            Log.KeyValuePair(Host.ApplicationAssemblyName, "Stopping...", Message.EContentType.ShutdownSequence);
            _appLifetime?.StopApplication();
        }
        private void OnStarted() { }
        private void OnStopped() { }
    }
}
