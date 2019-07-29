using System.Collections.Concurrent;
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

        [HttpGet("signout")]
        public IActionResult SignOut()
        {
            // Call the SignOut endpoint from the API, if in clientmode, or its own.

            Current.Context.Session.Clear();

            var url = App.Current.Orchestrator.GetApiUri() + "/framework/auth/signout";
            return new RedirectResult(url);
        }

        [HttpGet("maintenance/start")]
        public object DoMaintenance()
        {
            App.Current.Orchestrator.CompileAllPeoplePermissions();

            return true;
        }

        [HttpGet("identity")]
        public object GetIdentity()
        {
            var tmp = App.Current.Orchestrator.Person?.ToPropertyDictionary() ?? new ConcurrentDictionary<string, object>();

            tmp["IsAuthenticated"] = App.Current.Orchestrator.Person != null;

            return tmp;
        }
    }
}