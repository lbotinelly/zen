using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
//using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Zen.Base.Extension;
using Zen.Web.Auth.Configuration;

// ReSharper disable InconsistentlySynchronizedField
// ReSharper disable StaticMemberInGenericType
// ReSharper disable InconsistentNaming

namespace Zen.Web.Auth.Controller
{
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;

        private IConfiguration _config;


        public AuthController(SignInManager<IdentityUser> signInManager, UserManager<IdentityUser> userManager, IConfiguration config)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _config = config;
        }

        [HttpGet("signin")]
        public object SignIn([FromQuery] string provider, [FromQuery] string returnUrl)
        {
            returnUrl = returnUrl ?? Url.Content("~/");
            provider = provider ?? "google";

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
            ExternalLoginInfo info = await _signInManager.GetExternalLoginInfoAsync();

            if (info == null) return Problem("Error loading external login information during confirmation.");

            if (!ModelState.IsValid) return LocalRedirect(returnUrl);

            IdentityUser user = await _userManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey);

            var props = new AuthenticationProperties();
            props.StoreTokens(info.AuthenticationTokens);
            props.IsPersistent = true;

            //await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            //await _signInManager.SignInAsync(user, props);

            var identity = Current.AuthEventHandler?.OnConfirmSignIn(info.Principal.Identity) ?? info.Principal.Identity;

            var jwt = new JwtService();
            var token = jwt.GenerateSecurityToken((ClaimsIdentity)identity);
            Response.Cookies.Append("_zen_auth_bearer", token, new CookieOptions { IsEssential = true, Secure = true });

            Base.Log.KeyValuePair("Auth Token Issued", token.ToString());

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

        [Authorize]
        [HttpGet("identity")]
        public new Dictionary<string, string> GetIdentity()
        {
            var user = Current.AuthEventHandler?.GetIdentity(User).ToJson().FromJson<Dictionary<string, object>>();

            if (user == null) return null;

            var result = new Dictionary<string, string>
            {
                ["Email"] = user.TryGet("Email")?.ToString(),
                ["Id"] = user.TryGet("Id")?.ToString(),
                ["Name"] = user.TryGet("Name")?.ToString(),
                ["Roles"] = user.TryGet("Roles")?.ToString(),
            };

            return result;
        }

        [HttpGet("token")]
        public object GetToken()
        {
            return Request.Cookies.ContainsKey("_zen_auth_bearer") ? Request.Cookies["_zen_auth_bearer"] : null;
        }

        [HttpGet("providers")]
        public IEnumerable<AuthenticationScheme> GetProviders() => _signInManager.GetExternalAuthenticationSchemesAsync().Result.OrderBy(i=> i.Name);
    }

    public class JwtService
    {
        private readonly string _key;
        private readonly string _issuer;
        private readonly string _expDate;

        public JwtService()
        {
            _key = Base.Configuration.Options.GetSection("Authentication").GetSection("JWT").GetSection("key").Value;
            _issuer = Base.Configuration.Options.GetSection("Authentication").GetSection("JWT").GetSection("issuer").Value;
            _expDate = Base.Configuration.Options.GetSection("Authentication").GetSection("JWT").GetSection("expirationInMinutes").Value;
        }

        public string GenerateSecurityToken(ClaimsIdentity source)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_key);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = source,
                Expires = DateTime.UtcNow.AddMinutes(double.Parse(_expDate)),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
                Audience = _issuer
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);

        }
    }
}