using System.Collections.Generic;
using Bogus;
using Bogus.DataSets;
using Microsoft.AspNetCore.Mvc;
using SimpleWebApp.Models;
using Zen.Base.Module.Data;
using Zen.Module.Web.Controller;

namespace SimpleWebApp.Controllers
{
    [Route("api/[controller]"), ApiController, EndpointConfiguration.SecurityAttribute]
    public class SampleModelController : DataController<SampleModel>
    {
        [Route("addRandom")]
        public IEnumerable<SampleModel> AddRandom()
        {

            var temp = SampleModel.All();

            temp.Remove();

            var newUser = new Faker<SampleModel>()
                    .RuleFor(u => u.Gender, (f, u) => f.PickRandom<Name.Gender>())
                    .RuleFor(u => u.FirstName, (f, u) => f.Name.FirstName(u.Gender))
                    .RuleFor(u => u.LastName, (f, u) => f.Name.LastName(u.Gender))
                    .RuleFor(u => u.UserName, (f, u) => f.Internet.UserName(u.FirstName, u.LastName))
                    .RuleFor(u => u.Email, (f, u) => f.Internet.Email(u.FirstName, u.LastName))
                ;

            var buffer = new List<SampleModel>();


            for (var i = 0; i < 1000; i++) { buffer.Add(newUser.Generate()); }

            buffer.Save();

            return buffer;
        }
    }
}