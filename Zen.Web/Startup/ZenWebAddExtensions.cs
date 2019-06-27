using System;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Serialization;
using Zen.Base.Startup;
using Zen.Web.Service;

namespace Zen.Web.Startup
{
    public static class ZenWebAddExtensions
    {
        public static ZenWebBuilder AddZenWeb(this IServiceCollection services)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));

            services
                .AddZen();

            services
                .AddMvc()
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

            //services.AddSpaStaticFiles(configuration => { configuration.RootPath = "ClientApp/dist"; });

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            }).AddCookie();


            services.AddTransient<IEmailSender, EmailSender>();

            return new ZenWebBuilder(services);
        }

        public static ZenWebBuilder AddZenWeb(this IServiceCollection services, Action<ZenWebConfigureOptions> configureOptions)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));


            var builder = services.AddZenWeb();

            if (configureOptions != null) services.Configure(configureOptions);

            return builder;
        }
    }
}