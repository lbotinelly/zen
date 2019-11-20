using Microsoft.AspNetCore.Mvc;
using Zen.App.Model.Orchestration;
using Zen.App.Provider.Application;

namespace Zen.Web.Framework
{
    [Route("framework/configuration")]
    public class ConfigurationController : ControllerBase
    {
        [HttpGet("groups")]
        public ZenApplicationRepresentation GetGroups() { return App.Current.Orchestrator.Application?.ToRepresentation(); }

        [HttpPost("groups")]
        public ZenApplicationRepresentation SetGroups([FromBody] ZenApplicationRepresentation model)
        {
            App.Current.Orchestrator.Application.FromRepresentation(model);

            return App.Current.Orchestrator.Application?.ToRepresentation();
        }
    }
}