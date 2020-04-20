using Microsoft.Extensions.DependencyInjection;
using Zen.Base;
using Zen.Base.Extension;
using Zen.Base.Module.Log;
using Zen.Web.Auth;
using Zen.Web.Auth.Extensions;

namespace Zen.Provider.Microsoft.Authentication
{
    public static class Settings
    {
        private const string ProviderKey = "Microsoft";

        internal static IServiceCollection Configure(this IServiceCollection services)
        {
            if (Instances.Options?.WhitelistedProviders != null && Instances.Options?.WhitelistedProviders?.Contains(ProviderKey) != true) return services;

            var cid = Instances.Options?.Provider?.Val(ProviderKey)?.Val("ClientId");
            var cst = Instances.Options?.Provider?.Val(ProviderKey)?.Val("ClientSecret");

            if (cid == null || cst == null)
                Current.Log.KeyValuePair("Zen.Provider.Microsoft.Authentication", "Missing ClientId/ClientSecret", Message.EContentType.Warning);
            else
                Instances.AuthenticationBuilder.AddMicrosoftAccount(ProviderKey, options =>
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