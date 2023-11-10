using System.Linq;
using System.Threading;
using Microsoft.AspNetCore.Mvc;
using OpenGraphNet;
using Zen.Base.Extension;
using Zen.Media;

namespace Zen.Web.App.Media
{
    [Route("api/media/external")]
    public class MediaExternalController : ControllerBase
    {

        private static string _key = "Zen.Web.App.Media.MediaExternalController.Get";

        private readonly Mutex mutex = new(false, _key);

        // private Semaphore _pool = new Semaphore(1, 3, "Zen.Web.App.Media.MediaExternalController.Get");

        [HttpGet("")]
        [ResponseCache(Duration = 1 * 24 * 60 * 60, Location = ResponseCacheLocation.Any, NoStore = false)]
        public IActionResult Get()
        {
            mutex.WaitOne();

            var query = Request.Query;
            if (!query.ContainsKey("url")) return null;

            var url = query["url"];

            var dictQuery = query.OrderBy(i => i.Key).ToDictionary(i => i.Key, i => i.Value.ToString());


            try
            {
                var result = Helpers.GetAndCacheExternalResource(dictQuery);


                if (result == null)
                {
                    return NotFound();
                }

                return File(result.Stream, result.MimeType);
            }
            catch (System.Exception)
            {

                throw;
            }
            finally
            {
                mutex.ReleaseMutex();
            }
        }
    }
    [Route("api/media/openGraph/card")]
    public class MediaOpenGraphController : ControllerBase
    {
        [HttpGet("")]
        [ResponseCache(Duration = 1 * 60 * 60, Location = ResponseCacheLocation.Any, NoStore = false)]
        public object Get([FromQuery] string url)
        {
            OpenGraph graph = OpenGraph.ParseUrlAsync(url).Result;

            var data = new
            {
                graph.Url,
                graph.Title,
                graph.Image,
                graph.OriginalUrl,
                graph.Type,
                Description = graph.Metadata.FirstOrDefault(i=> i.Key == "og:description").Value.FirstOrDefault().Value
            };

            return data;
        }
    }
}


