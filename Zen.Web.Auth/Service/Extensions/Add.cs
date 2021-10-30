using System;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
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

            try
            {


                Instances.Options = Base.Configuration.Options.GetSection("Authentication").Get<Options>();

            }
            catch (Exception e)
            {

                throw;
            }


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
                .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.SlidingExpiration = true;
                    options.ExpireTimeSpan = TimeSpan.FromDays(30);
                });

            services.AddDataProtection()
                .PersistKeysToFileSystem(new DirectoryInfo(Path.Combine(Base.Host.DataDirectory, "keys")));

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