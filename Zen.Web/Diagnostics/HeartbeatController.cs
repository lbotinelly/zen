using Microsoft.AspNetCore.Mvc;

namespace Zen.Web.Diagnostics
{
    [Route("api/diagnostics/heartbeat")]
    public class HeartbeatController : ControllerBase
    {
        [HttpGet]
        public JsonResult Get() { return new JsonResult("its alive!"); }
    }
}