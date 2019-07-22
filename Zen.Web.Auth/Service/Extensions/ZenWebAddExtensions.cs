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
            if (services == null) throw new ArgumentNullException(nameof(services));

            configureOptions = configureOptions ?? (x => { });

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            services
                .AddIdentity<IdentityUser, IdentityRole>();

            services
                .AddAuthentication(options =>
                {
                    //options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    //options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    options.DefaultAuthenticateScheme = GoogleDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
                })
                //.AddCookie(options =>
                //{
                //    options.LoginPath = "/api/auth/signin";
                //    options.LogoutPath = "/api/auth/signout";
                //})
                .AddGoogle(options =>
                {
                    var googleAuthNSection = Configuration.Options.GetSection("Authentication:Google");
                    options.CallbackPath = "/api/auth/signin/google";

                    options.ClientId = googleAuthNSection["ClientId"];
                    options.ClientSecret = googleAuthNSection["ClientSecret"];
                });


            services
                .AddDistributedMemoryCache()
                .AddSession(options =>
                {
                    // Set a short timeout for easy testing.
                    options.IdleTimeout = TimeSpan.FromSeconds(10);
                    options.Cookie.HttpOnly = true;
                    // Make the session cookie essential
                    options.Cookie.IsEssential = true;
                });

            services.AddTransient<IEmailSender, EmailSender>();

            var builder = new Builder(services);

            if (configureOptions != null) services.Configure(configureOptions);

            return builder;
        }
    }
}