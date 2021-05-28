using Microsoft.AspNetCore.Mvc;
using Zen.App.Core.Application;

namespace Zen.Web.App.Framework
{
    [Route("framework/configuration")]
    public class ConfigurationController : ControllerBase
    {
        [HttpGet("groups")]
        public ApplicationRepresentation GetGroups() { return Zen.App.Current.Orchestrator.Application?.ToRepresentation(); }

        [HttpPost("groups")]
        public ApplicationRepresentation SetGroups([FromBody] ApplicationRepresentation model)
        {
            Zen.App.Current.Orchestrator.Application.FromRepresentation(model);

            return Zen.App.Current.Orchestrator.Application?.ToRepresentation();
        }
    }
}