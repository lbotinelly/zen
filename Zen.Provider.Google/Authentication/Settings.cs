using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.Extensions.DependencyInjection;
using Zen.Base;
using Zen.Web.Auth;

namespace Zen.Provider.Google.Authentication
{
    public static class Settings
    {
        internal static IServiceCollection Configure(this IServiceCollection services)
        {
            Instances.AuthenticationBuilder.AddGoogle(options =>
            {
                options.ClientId = Configuration.Options["Authentication:Google:ClientId"];
                options.ClientSecret = Configuration.Options["Authentication:Google:ClientSecret"];
                options.CallbackPath = "/auth/signin/google";

                options.UserInformationEndpoint = "https://www.googleapis.com/oauth2/v2/userinfo";
                options.ClaimActions.Clear();
                options.ClaimActions.MapJsonKey(ClaimTypes.NameIdentifier, "id");
                options.ClaimActions.MapJsonKey(ClaimTypes.Name, "name");
                options.ClaimActions.MapJsonKey(ClaimTypes.GivenName, "given_name");
                options.ClaimActions.MapJsonKey(ClaimTypes.Surname, "family_name");
                options.ClaimActions.MapJsonKey("urn:google:profile", "link");
                options.ClaimActions.MapJsonKey(ClaimTypes.Email, "email");
                options.Events = new OAuthEvents
                {
                    OnCreatingTicket = async context =>
                    {
                        var firstName = context.Identity.FindFirst(ClaimTypes.GivenName).Value;
                        var lastName = context.Identity.FindFirst(ClaimTypes.Surname)?.Value;
                        var email = context.Identity.FindFirst(ClaimTypes.Email).Value;

                        //Todo: Add logic here to save info into database

                        await Task.FromResult(true);
                    }
                };
            });

            return services;
        }
    }
}