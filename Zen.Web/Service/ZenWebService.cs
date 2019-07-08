using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Zen.Base.Service;

namespace Zen.Web.Service
{
    public class ZenWebService : IZenAutoService
    {
        public void Add(IServiceCollection services)
        {
            Service.Extensions.Add.AddZenWeb(services);
        }

        public void Use(IApplicationBuilder app, IHostingEnvironment env = null)
        {
            Service.Extensions.Use.UseZenWeb(app,null, env);
        }
    }
}