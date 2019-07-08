using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Text;

namespace Zen.Base.Service
{
    public interface IZenAutoService
    {
        void Add(IServiceCollection services);
        void Use(IApplicationBuilder app, IHostingEnvironment env = null);
    }
}
