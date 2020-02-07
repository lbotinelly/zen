using Microsoft.AspNetCore.Mvc;

namespace Zen.Web.Maintenance
{
    [Route("api/maintenance")]
    public class MaintenanceController : ControllerBase
    {
        [HttpGet("cache/flush")]
        public IActionResult FlushCache()
        {
            Base.Current.Cache.RemoveAll();
            return Ok("Cache flushed successfully.");
        }
    }
}