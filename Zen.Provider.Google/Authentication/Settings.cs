using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Zen.App.BaseAuth;
using Zen.Base;
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
                Current.Log.KeyValuePair("Zen.Provider.Google.Authentication", "Missing ClientId/ClientSecret", Message.EContentType.Warning);
            else
                Instances.AuthenticationBuilder.AddGoogle(ProviderKey, options =>
                {
                    options.ClientId = cid;
                    options.ClientSecret = cst;
                    options.CallbackPath = $"/auth/signin/{ProviderKey}";

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

            return services;
        }
    }
}