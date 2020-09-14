using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Sample04_AuthenticatedWeb.Model;
using Zen.Base.Module.Data;
using Zen.Web.Data.Controller;
using Zen.Web.Data.Controller.Attributes;

namespace Sample04_AuthenticatedWeb.Controllers
{
    [Route("data/samplePerson")]
    [DataBehavior(MustPaginate = true)]
    public class SamplePersonController : DataController<SamplePerson>
    {
        [HttpGet("generate")]
        public List<SamplePerson> Generate()
        {
            return SamplePerson.Generate().Save().Success;
        }
    }
}