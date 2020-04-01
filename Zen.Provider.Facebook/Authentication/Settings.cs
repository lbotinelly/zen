using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Zen.Base;
using Zen.Base.Module.Log;
using Zen.Web.Auth;
using Zen.Web.Auth.OAuth;

namespace Zen.Provider.Facebook.Authentication
{
    public static class Settings
    {
        internal static IServiceCollection Configure(this IServiceCollection services)
        {
            var aid = Configuration.Options["Authentication:Facebook:AppId"];
            var ast = Configuration.Options["Authentication:Facebook:AppSecret"];

            if (aid == null || ast == null)
                Current.Log.KeyValuePair("Zen.Provider.Facebook.Authentication", "Missing AppId/AppSecret", Message.EContentType.Warning);
            else
                Instances.AuthenticationBuilder.AddFacebook(options =>
                {
                    options.AppId = aid;
                    options.AppSecret = ast;
                    options.CallbackPath = "/auth/signin/facebook";

                    options.ClaimActions.MapJsonKey(ClaimTypes.GivenName, "name");

                    options.SaveTokens = true;

                    options.Events = Pipeline.EventHandler;

                });

            return services;
        }
    }
}