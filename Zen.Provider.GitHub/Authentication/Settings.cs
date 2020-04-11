using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Zen.App.BaseAuth;
using Zen.Base;
using Zen.Base.Module.Log;
using Zen.Web.Auth;
using Zen.Web.Auth.Extensions;
using Zen.Web.Auth.Model;

namespace Zen.Provider.GitHub.Authentication
{
    public static class Settings
    {
        internal static IServiceCollection Configure(this IServiceCollection services)
        {
            var cid = Configuration.Options["Authentication:GitHub:ClientId"];
            var cst = Configuration.Options["Authentication:GitHub:ClientSecret"];

            if (cid == null || cst == null)
                Current.Log.KeyValuePair("Zen.Provider.GitHub.Authentication", "Missing ClientId/ClientSecret", Message.EContentType.Warning);
            else
                Instances.AuthenticationBuilder.AddGitHub("Github", options =>
                {
                    options.ClientId = cid;
                    options.ClientSecret = cst;

                    options.Scope.Add("user:email");

                    options.CallbackPath = "/auth/signin/github";

                    options.TokenEndpoint = "https://github.com/login/oauth/access_token";
                    options.AuthorizationEndpoint = "https://github.com/login/oauth/authorize";
                    options.UserInformationEndpoint = "https://api.github.com/user";

                    options.ClaimActions.MapJsonKey(ClaimTypes.NameIdentifier, "id");
                    options.ClaimActions.MapJsonKey(ClaimTypes.Name, "login");
                    options.ClaimActions.MapJsonKey(ClaimTypes.GivenName, "name");
                    options.ClaimActions.MapJsonKey(ClaimTypes.Email, "email");
                    options.ClaimActions.MapJsonKey(ClaimTypes.Locality, "location");
                    options.ClaimActions.MapJsonKey(ClaimTypes.Webpage, "html_url");
                    options.ClaimActions.MapJsonKey(ZenClaimTypes.ProfilePicture, "picture", "url");
                    options.ClaimActions.MapJsonKey(ZenClaimTypes.Locale, "locale", "string");

                    options.SaveTokens = true;

                    options.Events = Pipeline.OAuthEventHandler;
                });

            return services;
        }
    }
}