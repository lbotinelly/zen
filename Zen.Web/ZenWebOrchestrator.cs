using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Session;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using Zen.Base.Common;
using Zen.Base.Module.Service;
using Zen.Web.Common;
using Zen.Web.Convention;
using Zen.Web.Host;
using Zen.Web.Model.State;
using Zen.Web.Service;

namespace Zen.Web
{
    public class ZenWebOrchestrator : IZenWebOrchestrator
    {
        public ZenWebOrchestrator(IOptions<Configuration.Options> options) : this(options.Value) { }

        public ZenWebOrchestrator(Configuration.Options options) => Options = options;

        public Configuration.Options Options { get; set; }
        public EOperationalStatus OperationalStatus { get; }

        public void Initialize()
        {
            var ctxConfig = Options.GetCurrentEnvironment();

            var appCode = App.Current.Configuration?.Code?.ToLower() ?? Base.Host.ApplicationAssemblyName;

            var usePrefix =
                ctxConfig.RoutePrefix != null ||
                ctxConfig.Behavior?.UseAppCodeAsRoutePrefix == true;

            var prefix =
                ctxConfig.RoutePrefix ??
                (ctxConfig.Behavior?.UseAppCodeAsRoutePrefix == true ? appCode : null);

            Base.Host.Variables[Keys.WebAppCode] = appCode;

            Base.Host.Variables[Keys.WebUsePrefix] = usePrefix;
            Base.Host.Variables[Keys.WebRootPrefix] = "/" + prefix;

            Base.Host.Variables[Keys.WebHttpPort] = ctxConfig.HttpPort;
            Base.Host.Variables[Keys.WebHttpsPort] = ctxConfig.HttpsPort;

            Base.Host.Variables[Keys.WebQualifiedServerName] = ctxConfig.QualifiedServerName;

            var services = Base.Module.Service.Instances.ServiceCollection;

            services.Configure<FormOptions>(options =>
            {
                options.MemoryBufferThreshold = int.MaxValue;
                options.MultipartBodyLengthLimit = long.MaxValue;
                options.MultipartBoundaryLengthLimit = int.MaxValue;
                options.ValueCountLimit = 10;
            });

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

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

            services
                .AddRazorPages();
            //.AddRazorRuntimeCompilation();

            services.AddSpaStaticFiles(configuration => { configuration.RootPath = "ClientApp/dist"; });

            services.AddTransient<IEmailSender, EmailSender>();

            services.AddTransient<ISessionStore, ZenDistributedSessionStore>();
        }

        public string GetState() => OperationalStatus.ToString();
    }
}