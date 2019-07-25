using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Zen.App.Provider;
using Zen.Base.Extension;

// ReSharper disable InconsistentlySynchronizedField
// ReSharper disable StaticMemberInGenericType
// ReSharper disable InconsistentNaming

namespace Zen.Web.Auth.Controller
{
    [Route("framework/auth"), ApiController]
    public class AuthController : Microsoft.AspNetCore.Mvc.Controller
    {
        [Authorize, HttpGet("signin")]
        public object SignIn()
        {
            var person = App.Current.Orchestrator.SigninPersonByIdentity(User.Identity);

            Base.Current.Log.Add(person.ToJson());

            return person;
        }

        [HttpGet("maintenance/start")]
        public object DoMaintenance()
        {
            App.Current.Orchestrator.CompileAllPeoplePermissions();

            return true;
        }
    }
}