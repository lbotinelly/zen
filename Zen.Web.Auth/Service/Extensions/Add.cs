using System;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Zen.Base;
using Zen.Base.Extension;
using Zen.Web.Auth.Configuration;
using Zen.Web.Auth.Identity;

namespace Zen.Web.Auth.Service.Extensions
{
    public static class Add
    {
        public static void AddZenWebAuth(this IServiceCollection services)
        {
            services.AddTransient<IUserStore<IdentityUser>, IdentityUserStore>();

            Instances.Options = Base.Configuration.Options.GetSection("Authentication").Get<Options>();

            Instances.Options?.Evaluate();

            Events.AddLog("Zen.Web.Auth", $"Mode: {Instances.Options?.Mode}, Providers: {Instances.Options?.Provider?.Select(i => i.Key).ToJson()}");

            try
            {
                services.AddDefaultIdentity<IdentityUser>(options =>
                {
                    options.SignIn.RequireConfirmedAccount = false;
                    options.SignIn.RequireConfirmedEmail = false;
                }).AddDefaultTokenProviders();

            }
            catch (Exception e)
            {
                Base.Current.Log.Add(e);
            }


            // services.AddRazorPages();

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