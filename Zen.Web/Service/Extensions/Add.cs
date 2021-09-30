using System;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using Zen.Base;

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

            services.AddMvc(options => { })
                .AddNewtonsoftJson(options =>
                {
                    //Json serializer settings Enum as string, omit nulls.
                    // https://gist.github.com/regisdiogo/27f62ef83a804668eb0d9d0f63989e3e
                    options.SerializerSettings.Converters.Add(new StringEnumConverter());
                    options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;

                    // Return JSON responses in LowerCase?
                    options.SerializerSettings.ContractResolver = new DefaultContractResolver();

                    // Resolve Looping navigation properties
                    options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                }); ;

            var builder = new ZenWebBuilder(services);

            return builder;
        }
    }
}