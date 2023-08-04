using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Zen.Web.Diagnostics
{
    public class ZenWebHealthCheck : IZenHealthCheck
    {
        public string Name => "Zen.Web";

        public HealthStatus? FailureStatus => HealthStatus.Unhealthy;

        public List<string> Tags => new() {"core"};

        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            //var payload = new Dictionary<string, object>();

            //foreach(var i in Base.Host.Variables)
            //{
            //    var probe = i.Value.ToJson(0,false, Newtonsoft.Json.Formatting.Indented);
            //    if (probe != null) payload.Add(i.Key, i.Value);
            //}

            return Task.FromResult(HealthCheckResult.Healthy("Operational"));
        }
    }
}
