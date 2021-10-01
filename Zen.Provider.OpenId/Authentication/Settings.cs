using Microsoft.Extensions.DependencyInjection;
using zenbase = Zen.Base;
using Zen.Base.Extension;
using Zen.Base.Module.Log;
using Zen.Web.Auth;
using Zen.Web.Auth.Handlers;
using Zen.Base.Common;
using Zen.Web.Auth.Model;
using System.Security.Principal;

namespace Geisinger.Api.Zen.OpenId.Provider.Authentication
{
    public static class Settings
    {
        private const string ProviderKey = "Geisinger";

        internal static IServiceCollection Configure(this IServiceCollection services)
        {
            if (Instances.Options?.WhitelistedProviders != null && Instances.Options?.WhitelistedProviders?.Contains(ProviderKey) != true)
            {
                zenbase::Current.Log.KeyValuePair("Geisinger.Api.Zen.OpenId.Provider", "Provider not Whitelisted", Message.EContentType.Warning);
                return services;
            }

            var config = (
                clientId: Instances.Options?.Provider?.Val(ProviderKey)?.Val("ClientId"),
                clientSecret: Instances.Options?.Provider?.Val(ProviderKey)?.Val("ClientSecret"),
                authority: Instances.Options?.Provider?.Val(ProviderKey)?.Val("Authority")
            );

            if (config.clientId == null || config.authority == null)
            {
                zenbase::Current.Log.KeyValuePair("Geisinger.Api.Zen.OpenId.Provider", "Missing ClientId/ClientSecret", Message.EContentType.Warning);
                return services;
            }

            if (config.authority == null)
            {
                zenbase::Current.Log.KeyValuePair("Geisinger.Api.Zen.OpenId.Provider", "Missing Authority", Message.EContentType.Warning);
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

    public class GeisingerAuthEventHandler : IAuthEventHandler
    {
        public bool OnboardIdentity(Identity model)
        {
            return true;
        }

        public EOperationalStatus OperationalStatus => EOperationalStatus.Initialized;
        public object GetIdentity() => null;
        public string GetSignOutRedirectUri() => null;
        public string GetState() => null;
        public void Initialize() { }
        public void OnConfirmSignIn(IIdentity identity) { }
        public object OnMaintenanceRequest() => null;
        public void OnSignOut() { }
    }
}