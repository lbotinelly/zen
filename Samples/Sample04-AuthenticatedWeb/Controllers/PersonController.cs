using Microsoft.AspNetCore.Mvc;
using Zen.App.Model.Core;
using Zen.Web.Data.Controller;

namespace Sample04_AuthenticatedWeb.Controllers
{
    [Route("data/auth/person")]
    public class PersonController : DataController<Person> { }
}