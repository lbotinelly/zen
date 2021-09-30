using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Zen.Web.Auth.Configuration;

// ReSharper disable InconsistentlySynchronizedField
// ReSharper disable StaticMemberInGenericType
// ReSharper disable InconsistentNaming

namespace Zen.Web.Auth.Controller
{
    [Route("auth")]
    public class AuthController : ControllerBase
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;

        public AuthController(SignInManager<IdentityUser> signInManager, UserManager<IdentityUser> userManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
        }

        [HttpGet("signin")]
        public object SignIn()
        {
            var returnUrl = Request.Query["sourceUrl"].FirstOrDefault() ?? Url.Content("~/");
            var provider = Request.Query["provider"].FirstOrDefault() ?? "Google";

            var postConfirmationUrl = $"confirm?returnUrl={WebUtility.UrlEncode(returnUrl)}";

            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, postConfirmationUrl);
            var challenge = new ChallengeResult(provider, properties);

            return challenge;
        }

        [HttpGet("signin/confirm")]
        public async Task<IActionResult> OnPostConfirmationAsync([FromQuery] string returnUrl = null)
        {
            returnUrl = returnUrl ?? Url.Content("~/");
            // Get the information about the user from the external login provider
            var info = await _signInManager.GetExternalLoginInfoAsync();

            if (info == null) return Problem("Error loading external login information during confirmation.");

            if (!ModelState.IsValid) return LocalRedirect(returnUrl);

            var user = await _userManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey);

            var props = new AuthenticationProperties();
            props.StoreTokens(info.AuthenticationTokens);
            props.IsPersistent = true;

            await _signInManager.SignInAsync(user, props);

            Current.AuthEventHandler?.OnConfirmSignIn(info.Principal.Identity);

            return LocalRedirect(returnUrl);
        }

        [HttpGet("signout")]
        public async Task<IActionResult> SignOut()
        {
            // Call the SignOut endpoint from the API, if in Client mode, or its own if in standalone.

            if (Instances.Options.Mode == Options.EMode.StandAlone)
            {
                await _signInManager.SignOutAsync();
                Current.AuthEventHandler?.OnSignOut();
                return Ok();
            }

            try
            {
                string targetUrl = Current.AuthEventHandler?.GetSignOutRedirectUri();
                return targetUrl != null ? LocalRedirect(targetUrl) : (IActionResult)Ok();
            }
            catch (Exception e)
            {
                Base.Current.Log.Add(e);
                return Problem(e.Message);
            }
        }

        [HttpGet("maintenance/start")]
        public object DoMaintenance()
        {
            return Current.AuthEventHandler?.OnMaintenanceRequest();
        }

        [HttpGet("identity")]
        public object GetIdentity()
        {
            return Current.AuthEventHandler?.GetIdentity();
        }

        [HttpGet("providers")]
        public IEnumerable<AuthenticationScheme> GetProviders() => _signInManager.GetExternalAuthenticationSchemesAsync().Result;
    }
}