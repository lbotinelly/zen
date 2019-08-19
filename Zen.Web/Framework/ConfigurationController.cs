using Microsoft.AspNetCore.Mvc;
using Zen.App.Model.Orchestration;

namespace Zen.Web.Framework
{
    [Route("framework/configuration"), ApiController]
    public class ConfigurationController : Controller
    {
        [HttpGet("groups")]
        public object GetGroups() { return App.Current.Orchestrator.Application?.ToRepresentation(); }
    }
}