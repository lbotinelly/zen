using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Zen.Base.Extension;
using Zen.Base.Module.Log;
using Zen.Web.Auth;
using Zen.Web.Auth.Extensions;

namespace Zen.Provider.Google.Authentication
{
    public static class Settings
    {
        private const string ProviderKey = "google";

        internal static IServiceCollection Configure(this IServiceCollection services)
        {

            if (Instances.Options?.WhitelistedProviders != null && Instances.Options?.WhitelistedProviders?.Contains(ProviderKey) != true) return services;

            var cid = Instances.Options?.Provider?.Val(ProviderKey)?.Val("ClientId");
            var cst = Instances.Options?.Provider?.Val(ProviderKey)?.Val("ClientSecret");

            if (cid == null || cst == null)
                Base.Current.Log.KeyValuePair("Zen.Provider.Google.Authentication", "Missing ClientId/ClientSecret", Message.EContentType.Warning);
            else
                Instances.AuthenticationBuilder.AddGoogle(ProviderKey, options =>
                {

                    options.Events.OnCreatingTicket = (arg) =>
                    {
                        

                        return Task.CompletedTask;
                    };



                    options.Events.OnRedirectToAuthorizationEndpoint = (arg) =>
                    {
                        if (!arg.RedirectUri.Contains("redirect_uri=https", StringComparison.OrdinalIgnoreCase))
                        {
                            arg.RedirectUri = arg.RedirectUri.Replace("redirect_uri=http", "redirect_uri=https", StringComparison.OrdinalIgnoreCase);
                        }

                        arg.HttpContext.Response.Redirect(arg.RedirectUri);

                        return Task.CompletedTask;
                    };

                    options.ClientId = cid;
                    options.ClientSecret = cst;

                    options.SignInScheme = Microsoft.AspNetCore.Identity.IdentityConstants.ExternalScheme;

                    options.CallbackPath = $"/api/auth/signin/{ProviderKey}";

                    options.UserInformationEndpoint = "https://www.googleapis.com/oauth2/v2/userinfo";

                    options.ClaimActions.Clear();
                    options.ClaimActions.MapJsonKey(ClaimTypes.NameIdentifier, "id");
                    options.ClaimActions.MapJsonKey(ClaimTypes.Name, "email");
                    options.ClaimActions.MapJsonKey(ClaimTypes.GivenName, "name");
                    options.ClaimActions.MapJsonKey(ClaimTypes.Surname, "family_name");
                    options.ClaimActions.MapJsonKey(ClaimTypes.Email, "email");
                    options.ClaimActions.MapJsonKey(ClaimTypes.Webpage, "link");
                    options.ClaimActions.MapJsonKey(ZenClaimTypes.ProfilePicture, "picture", "url");
                    options.ClaimActions.MapJsonKey(ZenClaimTypes.Locale, "locale", "string");
                    options.ClaimActions.MapJsonKey(ZenClaimTypes.EmailConfirmed, "email_confirmed");

                    options.SaveTokens = true;

                    options.Events = Pipeline.OAuthEventHandler;
                });

            //Cookie Policy needed for External Auth
            //services.Configure<CookiePolicyOptions>(options =>
            //{
            //    // This lambda determines whether user consent for non-essential cookies is needed for a given request.
            //    options.CheckConsentNeeded = context => true;
            //    options.MinimumSameSitePolicy = SameSiteMode.Unspecified;
            //});

            return services;
        }
    }
}