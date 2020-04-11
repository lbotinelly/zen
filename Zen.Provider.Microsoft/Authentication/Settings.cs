using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Zen.Base;
using Zen.Base.Module.Log;
using Zen.Web.Auth;
using Zen.Web.Auth.Extensions;

namespace Zen.Provider.Microsoft.Authentication
{
    public static class Settings
    {
        internal static IServiceCollection Configure(this IServiceCollection services)
        {
            var cid = Configuration.Options["Authentication:Microsoft:ClientId"];
            var cst = Configuration.Options["Authentication:Microsoft:ClientSecret"];

            if (cid == null || cst == null)
                Current.Log.KeyValuePair("Zen.Provider.Microsoft.Authentication", "Missing ClientId/ClientSecret", Message.EContentType.Warning);
            else
                Instances.AuthenticationBuilder.AddMicrosoftAccount(options =>
                {
                    options.ClientId = cid;
                    options.ClientSecret = cst;
                    options.CallbackPath = "/auth/signin/microsoft";

                    options.SaveTokens = true;

                    options.Events = Pipeline.OAuthEventHandler;
                });

            return services;
        }
    }
}