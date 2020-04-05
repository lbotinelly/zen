using System.Security.Claims;
using Microsoft.Extensions.DependencyInjection;
using Zen.Base;
using Zen.Base.Module.Log;
using Zen.Web.Auth;
using Zen.Web.Auth.OAuth;

namespace Zen.Provider.Steam.Authentication
{
    public static class Settings
    {
        internal static IServiceCollection Configure(this IServiceCollection services)
        {
            var akey = Configuration.Options["Authentication:Steam:ApplicationKey"];

            if (akey == null)
                Current.Log.KeyValuePair("Zen.Provider.Steam.Authentication", "Missing ApplicationKey", Message.EContentType.Warning);
            else
                Instances.AuthenticationBuilder.AddSteam(options =>
                {
                    options.ApplicationKey = akey;
                    options.CallbackPath = "/auth/signin/steam";

                    options.SaveTokens = true;

                    options.Events = Pipeline.OpenIdEventHandler;
                });

            return services;
        }
    }
}