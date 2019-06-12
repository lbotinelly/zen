using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Zen.Web.Internal;
using Zen.Web.Internal.Infrastructure;

namespace Zen.Web
{
    public class ZenServer : IServer
    {
        private readonly IServerAddressesFeature _serverAddresses;
        private readonly TaskCompletionSource<object> _stoppedTcs = new TaskCompletionSource<object>(TaskCreationOptions.RunContinuationsAsynchronously);

        private bool _hasStarted;
        private int _stopping;

        public ZenServer(IOptions<Setup.ZenServerOptions> options)
        {
            var logger = new LoggerFactory()
                .AddConsole()
                .AddDebug(); 

            CreateServiceContext(options, new LoggerFactory());
        }

        public Setup.ZenServerOptions Options => ServiceContext.ServerOptions;

        private ServiceContext ServiceContext { get; }

        private IZenTrace Trace => ServiceContext.Log;

        public IFeatureCollection Features { get; }

        public async Task StartAsync<TContext>(IHttpApplication<TContext> application, CancellationToken cancellationToken)
        {
            try
            {
                ValidateOptions();

                if (_hasStarted) throw new InvalidOperationException(CoreStrings.ServerAlreadyStarted);

                _hasStarted = true;

                async Task OnBind(ListenOptions options)
                {
                    // Add the HTTP middleware as the terminal connection middleware
                    //options.UseHttpServer(options.ConnectionAdapters, ServiceContext, application, options.Protocols);

                    var connectionDelegate = options.Build();

                    // Add the connection limit middleware
                    //if (Options.Limits.MaxConcurrentConnections.HasValue)
                    //{
                    //    connectionDelegate = new ConnectionLimitMiddleware(connectionDelegate, Options.Limits.MaxConcurrentConnections.Value, Trace).OnConnectionAsync;
                    //}

                    //var connectionDispatcher = new ConnectionDispatcher(ServiceContext, connectionDelegate);
                    //var transport = await _transportFactory.BindAsync(options.EndPoint).ConfigureAwait(false);

                    //// Update the endpoint
                    //options.EndPoint = transport.EndPoint;
                    //var acceptLoopTask = connectionDispatcher.StartAcceptingConnections(transport);

                    //_transports.Add((transport, acceptLoopTask));
                }
            } catch (Exception ex)
            {
                Trace.LogCritical(0, ex, CoreStrings.UnableToStart);
                Dispose();
                throw;
            }
        }

        // Graceful shutdown if possible
        public async Task StopAsync(CancellationToken cancellationToken)
        {
            if (Interlocked.Exchange(ref _stopping, 1) == 1)
            {
                await _stoppedTcs.Task.ConfigureAwait(false);
                return;
            }

            try { } catch (Exception ex)
            {
                _stoppedTcs.TrySetException(ex);
                throw;
            }

            _stoppedTcs.TrySetResult(null);
        }

        // Ungraceful shutdown
        public void Dispose()
        {
            var cancelledTokenSource = new CancellationTokenSource();
            cancelledTokenSource.Cancel();
            StopAsync(cancelledTokenSource.Token).GetAwaiter().GetResult();
        }

        private static ServiceContext CreateServiceContext(IOptions<Setup.ZenServerOptions> options, ILoggerFactory loggerFactory)
        {
            if (options == null) throw new ArgumentNullException(nameof(options));
            if (loggerFactory == null) throw new ArgumentNullException(nameof(loggerFactory));

            var serverOptions = options.Value ?? new Setup.ZenServerOptions();
            var logger = loggerFactory.CreateLogger("Zen");

            var trace = new ZenTrace(logger);

            return new ServiceContext
            {
                Log = trace,
                ServerOptions = serverOptions
            };
        }

        private void ValidateOptions()
        {
            //Options.ConfigurationLoader?.Load();
        }
    }
}