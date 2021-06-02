using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Zen.Web.Controller
{
    /// <summary>
    /// Provides cache maintenance methods.
    /// </summary>
    [Route("cache")]
    public class CacheController : ControllerBase
    {
        /// <summary>
        /// Clears all currently cached objects.
        /// </summary>
        /// <returns></returns>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [HttpGet("flush")]
        public JsonResult FlushCache()
        {
            Base.Current.Cache.RemoveAll();
            return new(new { message = "Cache flushed successfully." });
        }
    }
}