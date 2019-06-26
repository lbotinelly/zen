using System.Collections.Generic;
using Bogus;
using Bogus.DataSets;
using Microsoft.AspNetCore.Mvc;
using SimpleWebApp.Models;
using Zen.Base.Module.Data;
using Zen.Module.Web.REST.Controller;
using Zen.Module.Web.REST.Controller.Attributes;

namespace SimpleWebApp.Controllers
{
    [Route("api/[controller]"), ApiController, Behavior(MustPaginate = true)]
    public class SampleModelController : DataController<sampleModel>
    {
        [Route("generate/{count}")]
        public IEnumerable<sampleModel> Generate(int count)
        {
            var newUser = new Faker<sampleModel>()
                    .RuleFor(u => u.gender, (f, u) => f.PickRandom<Name.Gender>())
                    .RuleFor(u => u.firstName, (f, u) => f.Name.FirstName(u.gender))
                    .RuleFor(u => u.lastName, (f, u) => f.Name.LastName(u.gender))
                    .RuleFor(u => u.userName, (f, u) => f.Internet.UserName(u.firstName, u.lastName))
                    .RuleFor(u => u.email, (f, u) => f.Internet.Email(u.firstName, u.lastName))
                    .RuleFor(u => u.name, (f, u) => f.Name.FullName(u.gender))
                ;

            var buffer = new List<sampleModel>();

            for (var i = 0; i < count; i++) buffer.Add(newUser.Generate());

            buffer.Save();

            return buffer;
        }

        [Route("clear")]
        public IActionResult Clear()
        {
            sampleModel.RemoveAll();

            return new OkResult();
        }

        [Route("filter")]
        public IEnumerable<sampleModel> Filter() { return sampleModel.Where(i => i.email.Contains("oui")); }
    }
}