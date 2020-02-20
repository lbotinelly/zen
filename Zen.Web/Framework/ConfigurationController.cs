using Microsoft.AspNetCore.Mvc;
using Zen.App.Core.Application;

namespace Zen.Web.Framework
{
    [Route("framework/configuration")]
    public class ConfigurationController : ControllerBase
    {
        [HttpGet("groups")]
        public ApplicationRepresentation GetGroups() { return App.Current.Orchestrator.Application?.ToRepresentation(); }

        [HttpPost("groups")]
        public ApplicationRepresentation SetGroups([FromBody] ApplicationRepresentation model)
        {
            App.Current.Orchestrator.Application.FromRepresentation(model);

            return App.Current.Orchestrator.Application?.ToRepresentation();
        }
    }
}