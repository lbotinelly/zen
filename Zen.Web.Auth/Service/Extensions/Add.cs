using System;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
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
                Base.Current.Log.Add(e);
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

            var _key = Base.Configuration.Options.GetSection("Authentication").GetSection("JWT").GetSection("key").Value;
            var _issuer = Base.Configuration.Options.GetSection("Authentication").GetSection("JWT").GetSection("issuer").Value;

            Instances.AuthenticationBuilder = services
                .AddAuthentication(x=>
                {
                    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
            .AddJwtBearer(options =>
            {
                var sharedKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_key));
                var credentials = new SigningCredentials(sharedKey, SecurityAlgorithms.HmacSha256);
                options.IncludeErrorDetails = true;

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    IssuerSigningKey = sharedKey,
                    ValidateIssuer = false,
                    ValidateAudience = true,
                    ValidIssuer = _issuer,
                    ValidAudience = _issuer,
                };
            })
            ;



            services.AddDataProtection()
                .PersistKeysToFileSystem(new DirectoryInfo(Path.Combine(Base.Host.DataDirectory, "keys")));

            services
                .AddDistributedMemoryCache()
                //.AddSession(options =>
                //{
                //    options.IdleTimeout = TimeSpan.FromHours(12);
                //    options.Cookie.HttpOnly = true;
                //    options.Cookie.IsEssential = true;
                //    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
                //});

            ;
        }
    }
}