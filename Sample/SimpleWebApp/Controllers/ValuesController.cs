using Microsoft.AspNetCore.Mvc;
using SimpleWebApp.Models;
using Zen.Base.Common;
using Zen.Module.Web.Controller;

namespace SimpleWebApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [EndpointConfiguration.Security]
    public class SampleModelController : DataController<SampleModel>
    {
        [Route("addRandom")]
        public SampleModel addRandom()
        {
            var newO = new SampleModel {Name = ShortGuid.NewGuid().ToString()};
            newO.Save();
            return newO;
        }
    }
}