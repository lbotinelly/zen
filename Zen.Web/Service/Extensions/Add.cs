using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Session;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Newtonsoft.Json.Serialization;
using Zen.Base;
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
            var usePrefix = Current.Configuration?.RoutePrefix != null ||  Current.Configuration?.Behavior?.UseAppCodeAsRoutePrefix == true;
            var prefix = Current.Configuration?.RoutePrefix ?? (Current.Configuration?.Behavior?.UseAppCodeAsRoutePrefix == true ? appCode : null);

            services
                .AddSingleton<IHttpContextAccessor, HttpContextAccessor>()
                .AddMvc(options =>
                {
                    if (usePrefix)
                    {
                        options.UseCentralRoutePrefix(new RouteAttribute(prefix + "/"));
                    }
                })
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_2)
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
                })

                // "How to turn off or handle camelCasing in JSON response ASP.NET Core?"
                // https://stackoverflow.com/questions/38728200/how-to-turn-off-or-handle-camelcasing-in-json-response-asp-net-core
                .AddJsonOptions(opt => opt.SerializerSettings.ContractResolver = new DefaultContractResolver())
                //https://docs.microsoft.com/en-us/aspnet/core/web-api/advanced/formatting?view=aspnetcore-2.2
                .AddXmlSerializerFormatters()
                ;

            services.AddSpaStaticFiles(configuration => { configuration.RootPath = "ClientApp/dist"; });

            services.AddTransient<IEmailSender, EmailSender>();

            services.AddTransient<ISessionStore, ZenDistributedSessionStore>();

            var builder = new ZenWebBuilder(services);

            if (configureOptions != null) services.Configure(configureOptions);

            return builder;
        }
    }
}