using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.AspNetCore.Mvc;
using Zen.App.Provider.Person;

namespace Zen.Web.Framework.Data
{
    [Route("framework/data/person"), ApiController]
    public class PersonController : Controller
    {
        [HttpPost("profile/subset")]
        public List<IZenPersonProfile> GetProfiles()
        {

            var keySet = "";
            var req = Request;

            // Allows using several time the stream in ASP.Net Core
            req.EnableRewind();

            // Arguments: Stream, Encoding, detect encoding, buffer size 
            // AND, the most important: keep stream opened
            using (StreamReader reader
                = new StreamReader(req.Body, Encoding.UTF8, true, 1024, true))
            {
                keySet = reader.ReadToEnd();
            }

            // Rewind, so the core is not lost when it looks the body for the request
            req.Body.Position = 0;

            return App.Current.Orchestrator.GetProfiles(keySet);
        }
        [HttpGet("profile/subset")]
        public List<IZenPersonProfile> GetProfiles([FromQuery] string keys)
        {
            return App.Current.Orchestrator.GetProfiles(keys);
        }
    }
}