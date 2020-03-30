using System;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Zen.Web.Auth.Identity;

namespace Zen.Web.Auth.Service.Extensions
{
    public static class Add
    {
        public static void AddZenWebAuth(this IServiceCollection services)
        {


            //services.AddTransient<ApplicationSignInManager, ZenApplicationSignInManager>();

            //services.AddTransient<UserManager<IdentityUser>, Zen.Web.Auth.Identity.ApplicationUserManager>();
            //services.AddTransient<SignInManager<ApplicationUser>, ApplicationSignInManager>();
            //services.AddTransient<IPasswordHasher<ApplicationUser>, ApplicationUserPasswordHarsher>();
            services.AddTransient<IUserStore<IdentityUser>, IdentityUserStore>();
            //services.AddTransient<IUserClaimsPrincipalFactory<ApplicationUser>, ZenUserClaimsPrincipalFactory>();
            //services.AddTransient<IUserConfirmation<ApplicationUser>, ZenUserConfirmation>();


            //services.AddTransient<RoleManager<ApplicationRole>, ApplicationRoleManager>();
            services.AddTransient<IRoleStore<IdentityRole>, IdentityRoleStore>();

            //services.AddTransient<ILookupNormalizer, UpperInvariantLookupNormalizer>();
            //services.AddTransient<IdentityErrorDescriber, ZenIdentityErrorDescriber>();

            //services.AddIdentity<IdentityUser, IdentityRole>().AddDefaultTokenProviders();

            services.AddDefaultIdentity<IdentityUser>(options =>
            {
                options.SignIn.RequireConfirmedAccount = true;
                options.SignIn.RequireConfirmedEmail = true;
            }).AddDefaultTokenProviders(); ;


            services.AddRazorPages();


            Instances.AuthenticationBuilder = services
                .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options => { options.ExpireTimeSpan = TimeSpan.FromDays(7); });
        }
    }
}