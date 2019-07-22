using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Zen.Base.Extension;

// ReSharper disable InconsistentlySynchronizedField
// ReSharper disable StaticMemberInGenericType
// ReSharper disable InconsistentNaming

namespace Zen.Web.Auth.Controller
{
    [Route("framework/auth"), ApiController]
    public class AuthController : Microsoft.AspNetCore.Mvc.Controller
    {
        [Authorize]
        [HttpGet("signin")]
        public object SignIn()
        {
            Base.Current.Log.Add(this.User.ToString());

            return this.User;
        }
    }
}