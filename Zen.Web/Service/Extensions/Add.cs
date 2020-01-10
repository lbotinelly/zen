using System;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Session;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using Zen.Base.Module.Service;
using Zen.Web.Convention;
using Zen.Web.Model.State;

namespace Zen.Web.Service.Extensions
{
    public static class Add
    {
        public static ZenWebBuilder AddZenWeb(this IServiceCollection services, Action<ZenWebConfigureOptions> configureOptions = null)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));

            services.ResolveSettingsPackage();

            configureOptions = configureOptions ?? (x => { });

            var appCode = App.Current.Configuration?.Code?.ToLower();
            var usePrefix = Current.Configuration?.RoutePrefix != null || Current.Configuration?.Behavior?.UseAppCodeAsRoutePrefix == true;
            var prefix = Current.Configuration?.RoutePrefix ?? (Current.Configuration?.Behavior?.UseAppCodeAsRoutePrefix == true ? appCode : null);

            services.Configure<FormOptions>(options => { options.MemoryBufferThreshold = int.MaxValue; });

            services
                .AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            var mvc = services
                    .AddControllers(options =>
                    {
                        if (usePrefix) options.UseCentralRoutePrefix(new RouteAttribute(prefix + "/"));
                    })
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

                    })
                    // Disable inference rules
                    // https://docs.microsoft.com/en-us/aspnet/core/web-api/?view=aspnetcore-2.2
                    .ConfigureApiBehaviorOptions(options =>
                    {
                        //options.SuppressConsumesConstraintForFormFileParameters = true;
                        options.SuppressInferBindingSourcesForParameters = true;
                        //options.SuppressModelStateInvalidFilter = true;
                        //options.SuppressMapClientErrors = true;
                        //options.SuppressUseValidationProblemDetailsForInvalidModelStateResponses = true;
                        //options.ClientErrorMapping[404].Link =
                        //    "https://httpstatuses.com/404";
                    }) // "How to turn off or handle camelCasing in JSON response ASP.NET Core?"
                       // https://stackoverflow.com/questions/38728200/how-to-turn-off-or-handle-camelcasing-in-json-response-asp-net-core
                       .AddJsonOptions(opt =>
                       {
                           opt.JsonSerializerOptions.PropertyNamingPolicy = null;
                           opt.JsonSerializerOptions.IgnoreNullValues = true;
                           opt.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
                           opt.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                       })
                    .SetCompatibilityVersion(CompatibilityVersion.Version_3_0)
                    //https://docs.microsoft.com/en-us/aspnet/core/web-api/advanced/formatting?view=aspnetcore-3.0
                    .AddXmlSerializerFormatters()
                ;

            // .Net Core 3.0 requirement
            foreach (var entry in IoC.AssemblyLoadMap) mvc.AddApplicationPart(entry.Value).AddControllersAsServices();

            services.AddSpaStaticFiles(configuration => { configuration.RootPath = "ClientApp/dist"; });

            services.AddTransient<IEmailSender, EmailSender>();

            services.AddTransient<ISessionStore, ZenDistributedSessionStore>();

            var builder = new ZenWebBuilder(services);

            if (configureOptions != null) services.Configure(configureOptions);

            return builder;
        }
    }
}