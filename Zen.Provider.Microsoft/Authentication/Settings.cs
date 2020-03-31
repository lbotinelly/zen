using Microsoft.Extensions.DependencyInjection;
using Zen.Base;
using Zen.Web.Auth;

namespace Zen.Provider.Microsoft.Authentication
{
    public static class Settings
    {
        internal static IServiceCollection Configure(this IServiceCollection services)
        {
            Instances.AuthenticationBuilder.AddMicrosoftAccount(options =>
            {
                options.ClientId = Configuration.Options["Authentication:Microsoft:ClientId"];
                options.ClientSecret = Configuration.Options["Authentication:Microsoft:ClientSecret"];
                options.CallbackPath = "/auth/signin/microsoft";
            });

            return services;
        }
    }
}