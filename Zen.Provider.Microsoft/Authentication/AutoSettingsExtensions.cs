using Microsoft.Extensions.DependencyInjection;
using Zen.Base;
using Zen.Web.Auth;

namespace Zen.Provider.Microsoft.Authentication
{
    public static class MicrosoftAuthentication
    {
        internal static IServiceCollection ResolveSettingsPackage(this IServiceCollection services)
        {
            //services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));
            // services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true);

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