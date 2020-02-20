using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Zen.App.Core.Person;

namespace Zen.Web.Framework.Data
{
    [Route("framework/data/person")]
    public class PersonController : ControllerBase
    {
        [HttpPost("profile/subset")]
        public List<IPersonProfile> GetProfiles()
        {
            var personKeySet = "";
            var request = Request;

            // Allows using several time the stream in ASP.Net Core
            request.EnableBuffering();

            // Arguments: Stream, Encoding, detect encoding, buffer size 
            // AND, the most important: keep stream opened
            using (var reader = new StreamReader(request.Body, Encoding.UTF8, true, 1024, true)) { personKeySet = reader.ReadToEndAsync().Result; }

            // Rewind, so the core is not lost when it looks the body for the request
            request.Body.Position = 0;

            var profiles = App.Current.Orchestrator.GetProfiles(personKeySet);

            return profiles;
        }

        [HttpGet("profile/subset")]
        public List<IPersonProfile> GetProfiles([FromQuery] string keys)
        {
            var profiles = App.Current.Orchestrator.GetProfiles(keys);
            return profiles;
        }
    }
}