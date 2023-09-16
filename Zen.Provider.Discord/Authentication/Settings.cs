using System.Globalization;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Zen.Base.Extension;
using Zen.Base.Module.Log;
using Zen.Web.Auth;
using Zen.Web.Auth.Extensions;

namespace Zen.Provider.Discord.Authentication
{
    public static class Settings
    {
        private const string ProviderKey = "discord";

        internal static IServiceCollection Configure(this IServiceCollection services)
        {

            if (Instances.Options?.WhitelistedProviders != null && Instances.Options?.WhitelistedProviders?.Contains(ProviderKey) != true) return services;

            var cid = Instances.Options?.Provider?.Val(ProviderKey)?.Val("ClientId");
            var cst = Instances.Options?.Provider?.Val(ProviderKey)?.Val("ClientSecret");

            if (cid == null || cst == null)
                Base.Current.Log.KeyValuePair("Zen.Provider.Discord.Authentication", "Missing ClientId/ClientSecret", Message.EContentType.Warning);
            else
                _ = Instances.AuthenticationBuilder.AddDiscord(ProviderKey, options =>
                {
                    options.ClientId = cid;
                    options.ClientSecret = cst;

                    options.ClaimActions.MapCustomJson("urn:discord:avatar:url", user =>
                    {

                        return string.Format(
                            CultureInfo.InvariantCulture,
                            "https://cdn.discordapp.com/avatars/{0}/{1}.{2}",
                            user.GetString("id"),
                            user.GetString("avatar"),
                            user.GetString("avatar").StartsWith("a_") ? "gif" : "png");
                    });

                    options.CallbackPath = $"/api/auth/signin/{ProviderKey}";


                    options.ClaimActions.MapCustomJson(ClaimTypes.GivenName, user =>
                    {
                        return user.GetString("global_name");
                    });

                    options.ClaimActions.MapCustomJson(ClaimTypes.Name, user =>
                    {
                        return user.GetString("username");
                    });


                    options.ClaimActions.MapCustomJson(ZenClaimTypes.ProfilePicture, user =>
                    {

                        var result = string.Format(CultureInfo.InvariantCulture,
                                            "https://cdn.discordapp.com/avatars/{0}/{1}.{2}",
                                            user.GetString("id"),
                                            user.GetString("avatar"),
                                            user.GetString("avatar").StartsWith("a_") ? "gif" : "png");

                        return result;
                    });

                    options.SaveTokens = true;

                    options.Events = Pipeline.OAuthEventHandler;
                });

            //Cookie Policy needed for External Auth
            //services.Configure<CookiePolicyOptions>(options =>
            //{
            //    // This lambda determines whether user consent for non-essential cookies is needed for a given request.
            //    options.CheckConsentNeeded = context => true;
            //    options.MinimumSameSitePolicy = SameSiteMode.Unspecified;
            //});

            return services;
        }
    }
}