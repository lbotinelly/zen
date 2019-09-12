using Microsoft.AspNetCore.Mvc;

namespace Zen.Web.Diagnostics
{
    [Route("api/diagnostics/heartbeat"), ApiController]
    public class HeartbeatController : Controller
    {
        [HttpGet]
        public JsonResult Get() { return Json("its alive!"); }
    }
}