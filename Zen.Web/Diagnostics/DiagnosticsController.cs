using System.Linq;
using Microsoft.AspNetCore.Mvc;

namespace Zen.Web.Diagnostics
{
    [Route("api/diagnostics")]
    public class DiagnosticsController : ControllerBase
    {
        [HttpGet("heartbeat")]
        public JsonResult GetHeartbeat() { return new JsonResult("its alive!"); }

        [HttpGet("variables")]
        public object GetVariables() { return Base.Host.Variables.ToDictionary(i=> i.Key, i=> i.Value.ToString()); }
    }
}