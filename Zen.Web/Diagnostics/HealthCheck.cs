using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Threading.Tasks;
using Zen.Base.Extension;

namespace Zen.Web.Diagnostics
{
    public static class HealthCheck
    {
        public static Task WriteResponse(HttpContext context, HealthReport healthReport)
        {
            context.Response.ContentType = "application/json; charset=utf-8";
            var payload = healthReport.ToJson(0, true, Newtonsoft.Json.Formatting.Indented, true);
            return context.Response.WriteAsync(payload);
        }
    }
}
