using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Collections.Generic;

namespace Zen.Web.Diagnostics
{
    public interface IZenHealthCheck : IHealthCheck
    {
        string Name { get; }
        HealthStatus? FailureStatus { get; }
        List<string> Tags { get; }
    }
}
