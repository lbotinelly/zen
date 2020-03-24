using Microsoft.AspNetCore.Mvc;
using Sample02_REST.Model;
using Zen.Web.Data.Controller;

namespace Sample02_REST.Controller
{
    [Route("api/people")]
    public class PersonController : DataController<Person>
    {
    }
} 