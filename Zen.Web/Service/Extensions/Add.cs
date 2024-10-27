using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Cors;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using Zen.Base;
using Zen.Base.Extension;
using Zen.Base.Module.Service;
using Zen.Web.Diagnostics;

namespace Zen.Web.Service.Extensions
{
    public static class Add
    {
        public static ZenWebBuilder AddZenWeb(this IServiceCollection services)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));

            services.ResolveSettingsPackage();
            services.Configure<Configuration.IOptions>(options =>
            {
                options.GetSettings<Configuration.IOptions, Configuration.IOptions>("Web");
            });

            services.AddCors(o => o.AddDefaultPolicy(builder =>
            {
                builder
                    .AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader();
            }));

            var mvcBuilder = services.AddMvc(options => { });
            mvcBuilder.AddNewtonsoftJson(options =>
                {
                    //Json serializer settings Enum as string, omit nulls.
                    // https://gist.github.com/regisdiogo/27f62ef83a804668eb0d9d0f63989e3e
                    options.SerializerSettings.Converters.Add(new StringEnumConverter());
                    options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;

                    // Return JSON responses in LowerCase?
                    options.SerializerSettings.ContractResolver = new DefaultContractResolver();

                    // Resolve Looping navigation properties
                    options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                });

            var hcBuilder = services.AddHealthChecks();
            //Load all automatic custom healthcheck implementations and instance

            var hcTypes = IoC.GetClassesByInterface<IZenHealthCheck>(false);
            var hcInstances = hcTypes.CreateInstances<IZenHealthCheck>().ToList();

            hcInstances.ForEach(i => hcBuilder.AddCheck(i.Name, i, i.FailureStatus, i.Tags));

            services.AddHeaderPropagation(options =>
            {
                options.Headers.Add("X-Correlation-ID");
            });

            var builder = new ZenWebBuilder(services);

            return builder;
        }
    }
}