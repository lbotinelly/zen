using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Zen.App.Provider.Person;
using Zen.Base;

namespace Zen.Web.Framework.Data
{
    [Route("framework/data/person")]
    public class PersonController : ControllerBase
    {
        [HttpPost("profile/subset")]
        public List<IZenPersonProfile> GetProfiles()
        {
            var keySet = "";
            var req = Request;

            // Allows using several time the stream in ASP.Net Core
            req.EnableBuffering();

            // Arguments: Stream, Encoding, detect encoding, buffer size 
            // AND, the most important: keep stream opened
            using (var reader = new StreamReader(req.Body, Encoding.UTF8, true, 1024, true)) { keySet = reader.ReadToEndAsync().Result; }

            // Rewind, so the core is not lost when it looks the body for the request
            req.Body.Position = 0;

            return App.Current.Orchestrator.GetProfiles(keySet);
        }

        [HttpGet("profile/subset")]
        public List<IZenPersonProfile> GetProfiles([FromQuery] string keys) { return App.Current.Orchestrator.GetProfiles(keys); }
    }
}