using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Zen.Base;
using Zen.Base.Extension;
using Zen.Base.Module.Log;
using Zen.Web.Auth;
using Zen.Web.Auth.Extensions;

namespace Zen.Provider.Facebook.Authentication
{
    public static class Settings
    {
        private const string ProviderKey = "Facebook";

        internal static IServiceCollection Configure(this IServiceCollection services)
        {
            if (Instances.Options.WhitelistedProviders != null && !Instances.Options.WhitelistedProviders.Contains(ProviderKey)) return services;

            var aid = Instances.Options.Provider.Val(ProviderKey)?.Val("AppId");
            var ast = Instances.Options.Provider.Val(ProviderKey)?.Val("AppSecret");

            if (aid == null || ast == null)
                Current.Log.KeyValuePair("Zen.Provider.Facebook.Authentication", "Missing AppId/AppSecret", Message.EContentType.Warning);
            else
                Instances.AuthenticationBuilder.AddFacebook(ProviderKey, options =>
                {
                    options.AppId = aid;
                    options.AppSecret = ast;
                    options.CallbackPath = "/auth/signin/facebook";

                    //options.ClaimActions.Clear();
                    options.ClaimActions.MapJsonKey(ClaimTypes.GivenName, "name");
                    options.ClaimActions.MapJsonKey(ClaimTypes.Email, "email");
                    options.ClaimActions.MapJsonKey(ClaimTypes.Surname, "last_name");

                    options.SaveTokens = true;

                    options.Events = Pipeline.OAuthEventHandler;
                });

            return services;
        }
    }
}