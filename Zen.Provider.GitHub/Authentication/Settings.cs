using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Zen.App.BaseAuth;
using Zen.Base;
using Zen.Base.Extension;
using Zen.Base.Module.Log;
using Zen.Web.Auth;
using Zen.Web.Auth.Extensions;

namespace Zen.Provider.GitHub.Authentication
{
    public static class Settings
    {
        private const string ProviderKey = "GitHub";

        internal static IServiceCollection Configure(this IServiceCollection services)
        {
            if (Instances.Options?.WhitelistedProviders != null && !Instances.Options?.WhitelistedProviders?.Contains(ProviderKey) == true) return services;

            var cid = Instances.Options?.Provider?.Val(ProviderKey)?.Val("ClientId");
            var cst = Instances.Options?.Provider?.Val(ProviderKey)?.Val("ClientSecret");

            if (cid == null || cst == null)
                Current.Log.KeyValuePair("Zen.Provider.GitHub.Authentication", "Missing ClientId/ClientSecret", Message.EContentType.Warning);
            else
                Instances.AuthenticationBuilder.AddGitHub(ProviderKey, options =>
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