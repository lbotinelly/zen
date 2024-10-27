using System.IO;
using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Zen.Web
{
    public static class RestExtensions
    {
        public static string BodyContent(this HttpRequest source)
        {
            string payload = null;

            using (var mem = new MemoryStream())
            using (var reader = new StreamReader(mem))
            {
                source.Body.CopyToAsync(mem).Wait();

                mem.Seek(0, SeekOrigin.Begin);
                payload = reader.ReadToEnd();

            }

            return payload;
        }

        public static IActionResult FromObject(this HttpResponse response, object content = null, HttpStatusCode status = HttpStatusCode.OK)
        {
            var result = new ObjectResult(content) { StatusCode = (int)status };
            return result;
        }

        public static IActionResult FromStatus(this HttpResponse response, HttpStatusCode status)
        {
            var result = new ObjectResult(new { status }) { StatusCode = (int)HttpStatusCode.OK };
            return result;
        }

        public static IActionResult Success(this HttpResponse response, object content = null)
        {
            if (content == null) content = new { Message = "Success" };

            var result = new ObjectResult(content) { StatusCode = (int)HttpStatusCode.OK };
            return result;
        }
    }
}