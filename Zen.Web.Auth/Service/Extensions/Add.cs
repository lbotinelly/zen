using System;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Zen.Web.Auth.Identity;

namespace Zen.Web.Auth.Service.Extensions
{
    public static class Add
    {
        public static void AddZenWebAuth(this IServiceCollection services)
        {
            services.AddTransient<IUserStore<IdentityUser>, IdentityUserStore>();

            services.AddDefaultIdentity<IdentityUser>(options =>
            {
                options.SignIn.RequireConfirmedAccount = false;
                options.SignIn.RequireConfirmedEmail = false;
            }).AddDefaultTokenProviders();

            services.AddRazorPages();

            Instances.AuthenticationBuilder = services
                .AddAuthentication(options =>
                {
                    //options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    //options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    //options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                })
                .AddCookie(options =>
                {
                    //options.LoginPath = "/Account/Unauthorized/";
                    //options.AccessDeniedPath = "/Account/Forbidden/";
                    options.ExpireTimeSpan = TimeSpan.FromDays(7);
                });

                services
                .AddDistributedMemoryCache()
                .AddSession(options =>
                {
                    options.IdleTimeout = TimeSpan.FromHours(12);
                    options.Cookie.HttpOnly = true;
                    options.Cookie.IsEssential = true;
                    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
                });

            ;
        }
    }
}