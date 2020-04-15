using Microsoft.AspNetCore.Mvc;
using Zen.Web.Auth.Model;
using Zen.Web.Data.Controller;

namespace Sample04_AuthenticatedWeb.Controllers {
    [Route("data/auth/identity")]
    public class ProviderIdentityUserController : DataController<Identity> { }
}