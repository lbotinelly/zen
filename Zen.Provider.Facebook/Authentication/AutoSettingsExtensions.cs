﻿using Microsoft.Extensions.DependencyInjection;
using Zen.Base;
using Zen.Web.Auth;

namespace Zen.Provider.Facebook.Authentication
{
    public static class MicrosoftAuthentication
    {
        internal static IServiceCollection ResolveSettingsPackage(this IServiceCollection services)
        {
            Instances.AuthenticationBuilder.AddFacebook(options =>
            {
                options.AppId = Configuration.Options["Authentication:Facebook:AppId"];
                options.AppSecret = Configuration.Options["Authentication:Facebook:AppSecret"];
                options.CallbackPath = "/auth/signin/facebook";
            });

            return services;
        }
    }
}