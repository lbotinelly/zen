using System;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Zen.Base;

namespace Zen.Provider.Google.Authentication
{
    public static class GoogleAuthentication
    {
        internal static IServiceCollection AddAuthenticationProvider(this IServiceCollection services)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));

            services.AddIdentity<IdentityUser, IdentityRole>();

            // Do we have Google stuff?
            var googleAuthNSection = Configuration.Options.GetSection("Authentication:Google");

            var googleClientId = googleAuthNSection["ClientId"];
            var googleClientSecret = googleAuthNSection["ClientSecret"];

            if (googleClientId != null)

                services
                    .AddAuthentication(options =>
                    {
                        options.DefaultAuthenticateScheme = GoogleDefaults.AuthenticationScheme;
                        options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
                    })
                    .AddGoogle(options =>
                    {
                        options.CallbackPath = "/auth/signin/google";
                        options.ClientId = googleClientId;
                        options.ClientSecret = googleClientSecret;
                    });
            else
                services
                    .AddAuthentication(options =>
                    {
                        options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                        options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    })
                    .AddCookie(options =>
                    {
                        options.LoginPath = "/auth/signin";
                        options.LogoutPath = "/auth/signout";
                    });

            services
                .AddDistributedMemoryCache()
                .AddSession(options =>
                {
                    options.IdleTimeout = TimeSpan.FromHours(6);
                    options.Cookie.HttpOnly = true;
                    options.Cookie.IsEssential = true;
                });

            return services;
        }
    }
}