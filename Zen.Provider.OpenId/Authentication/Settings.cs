using Microsoft.Extensions.DependencyInjection;
using zenbase = Zen.Base;
using Zen.Base.Extension;
using Zen.Base.Module.Log;
using Zen.Web.Auth;

namespace Zen.Provider.OpenId.Authentication
{
    public static class Settings
    {
        private const string ProviderKey = "OpenId";

        internal static IServiceCollection Configure(this IServiceCollection services)
        {
            if (Instances.Options?.WhitelistedProviders != null && Instances.Options?.WhitelistedProviders?.Contains(ProviderKey) != true)
            {
                zenbase::Current.Log.KeyValuePair("Zen.Provider.OpenId", "Provider not Whitelisted", Message.EContentType.Warning);
                return services;
            }

            var config = (
                clientId: Instances.Options?.Provider?.Val(ProviderKey)?.Val("ClientId"),
                clientSecret: Instances.Options?.Provider?.Val(ProviderKey)?.Val("ClientSecret"),
                authority: Instances.Options?.Provider?.Val(ProviderKey)?.Val("Authority")
            );

            if (config.clientId == null || config.authority == null)
            {
                zenbase::Current.Log.KeyValuePair("Zen.Provider.OpenId", "Missing ClientId/ClientSecret", Message.EContentType.Warning);
                return services;
            }

            if (config.authority == null)
            {
                zenbase::Current.Log.KeyValuePair("Zen.Provider.OpenId", "Missing Authority", Message.EContentType.Warning);
                return services;
            }

            Instances.AuthenticationBuilder.AddOpenIdConnect(ProviderKey, options =>
                {
                    options.ClientId = config.clientId;
                    options.ClientSecret = config.clientSecret;
                    options.CallbackPath = $"/auth/signin/{ProviderKey}";
                    options.Authority = config.authority;
                    options.RequireHttpsMetadata = false;
                    options.ResponseType = "id_token token";

                    options.Scope.Add("openid");
                    options.Scope.Add("profile");
                    options.Scope.Add("email");

                    // options.ClaimActions.Clear();
                    options.SaveTokens = true;
                })
                .AddJwtBearer();

            return services;
        }
    }
}