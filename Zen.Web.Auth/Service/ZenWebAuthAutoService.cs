﻿using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Zen.Base.Common;
using Zen.Base.Module.Service;
using Zen.Web.Auth.Service.Extensions;

namespace Zen.Web.Auth.Service
{
    [Priority(Level = 1)]
    public class ZenWebAuthAutoService : IZenAutoUseService, IZenAutoAddService
    {
        public void Use(IApplicationBuilder app, IHostEnvironment env = null)
        {
            app.UseZenWebAuth();
        }

        public void Add(IServiceCollection services)
        {
            services.AddZenWebAuth();
        }

        public void Use(IHost app, IHostEnvironment env = null)
        {
            throw new System.NotImplementedException("Zen.Web.Auth: IHostEnvironment not supported");
        }
    }
}