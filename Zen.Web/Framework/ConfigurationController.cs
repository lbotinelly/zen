using Microsoft.AspNetCore.Mvc;
using Zen.App.Orchestrator.Model;

namespace Zen.Web.Framework
{
    [Route("framework/configuration"), ApiController]
    public class ConfigurationController : Controller
    {
        [HttpGet("groups")]
        public object GetGroups() { return App.Current.Orchestrator.Application?.ToRepresentation(); }
    }
}