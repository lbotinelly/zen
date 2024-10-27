using Microsoft.AspNetCore.Mvc;
using Sample02_REST.Model;
using Zen.Web.Data.Controller;

namespace Sample02_REST.Controller
{
    [Route("person")]
    public class PersonController : DataController<Person>
    {
        [HttpGet("random")]
        public bool GenerateRandom()
        {
            var rc = Person.Count();

            if (rc == 0)
                for (var i = 1; i < 10; i++)
                {
                    var r = Person.Random();
                    r.Id = i.ToString();
                    r.Save();
                }

            return true;
        }
    }
} 