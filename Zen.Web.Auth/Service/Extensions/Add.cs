using System;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.DependencyInjection;
using Zen.Base;
using Zen.Web.Service;

namespace Zen.Web.Auth.Service.Extensions
{
    public static class Add
    {
        public static Builder AddZenWebAuth(this IServiceCollection services, Action<ZenWebConfigureOptions> configureOptions = null)
        {
            services.ResolveSettingsPackage();

            if (services == null) throw new ArgumentNullException(nameof(services));

            configureOptions = configureOptions ?? (x => { });

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            services
                .AddIdentity<IdentityUser, IdentityRole>();

            // Do we have Google stuff?
            var googleAuthNSection = Configuration.Options.GetSection("Authentication:Google");

            var googleClientId = googleAuthNSection["ClientId"];
            var googleClientSecret = googleAuthNSection["ClientSecret"];

            if (googleClientId!= null)
            {
                Settings.IsAuthProvider = true;

                services
                    .AddAuthentication(options =>
                    {
                        options.DefaultAuthenticateScheme = GoogleDefaults.AuthenticationScheme;
                        options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
                    })
                    .AddGoogle(options =>
                    {
                        options.CallbackPath = "/api/auth/signin/google";
                        options.ClientId = googleClientId;
                        options.ClientSecret = googleClientSecret;
                    });
            }
            else
            {
                services
                    .AddAuthentication(options =>
                    {
                        options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                        options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    })
                    .AddCookie(options =>
                    {
                        options.LoginPath = "/api/auth/signin";
                        options.LogoutPath = "/api/auth/signout";
                    });
            }

            services
                .AddDistributedMemoryCache()
                .AddSession(options =>
                {
                    options.IdleTimeout = TimeSpan.FromHours(6);
                    options.Cookie.HttpOnly = true;
                    options.Cookie.IsEssential = true;
                });

            services.AddTransient<IEmailSender, EmailSender>();

            var builder = new Builder(services);

            if (configureOptions!= null) services.Configure(configureOptions);

            return builder;
        }
    }
}