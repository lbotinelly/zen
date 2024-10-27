using System.Security.Claims;
using Microsoft.Extensions.DependencyInjection;
using Zen.Base.Extension;
using Zen.Base.Module.Log;
using Zen.Web.Auth;
using Zen.Web.Auth.Extensions;

namespace Zen.Provider.Steam.Authentication
{
    public static class Settings
    {
        private const string ProviderKey = "steam";

        internal static IServiceCollection Configure(this IServiceCollection services)
        {
            if (Instances.Options.WhitelistedProviders != null && !Instances.Options.WhitelistedProviders.Contains(ProviderKey)) return services;

            var applicationKey = Instances.Options.Provider.Val(ProviderKey)?.Val("ApplicationKey");

            if (applicationKey == null)
                Base.Current.Log.KeyValuePair("Zen.Provider.Steam.Authentication", "Missing ApplicationKey", Message.EContentType.Warning);
            else
                Instances.AuthenticationBuilder.AddSteam(options =>
                {
                    options.ApplicationKey = applicationKey;
                    options.CallbackPath = "/api/auth/signin/steam";

                    options.SaveTokens = true;

                    options.Events = Pipeline.OpenIdEventHandler;

                    options.ClearClaimMap();
                    options.MapClaimToJsonKey(ClaimTypes.Name, "response.players[0].realname");
                    options.MapClaimToJsonKey(ClaimTypes.GivenName, "response.players[0].personaname");
                    options.MapClaimToJsonKey(ClaimTypes.Webpage, "response.players[0].profileurl");
                    options.MapClaimToJsonKey(ZenClaimTypes.ProfilePicture, "response.players[0].avatarfull");
                    options.MapClaimToJsonKey(ZenClaimTypes.Locale, "response.players[0].loccountrycode");
                });

            return services;
        }
    }
}