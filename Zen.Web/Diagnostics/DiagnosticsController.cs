using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Zen.Web.Diagnostics
{
    [Route("api/diagnostics")]
    public class DiagnosticsController : ControllerBase
    {
        /// <summary>
        /// The heartbeat endpoint verifies that the service is available.
        /// </summary>
        /// <response code="200">The service is available.</response>
        [HttpGet("heartbeat")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        public JsonResult GetHeartbeat() { return new JsonResult("its alive!"); }

        /// <summary>
        /// Returns all host variables.
        /// </summary>
        /// <response code="200">Host variables compiled successfully.</response>
        [HttpGet("variables")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        public object GetVariables() => Base.Host.Variables.ToDictionary(i => i.Key, i => i.Value?.ToString());
    }
}