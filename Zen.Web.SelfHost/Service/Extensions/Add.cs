using System.IO;
using LettuceEncrypt;
using Microsoft.Extensions.DependencyInjection;
using Zen.Base;

namespace Zen.Web.SelfHost.Service.Extensions
{
    public static class Add
    {
        public static void AddZenWebSelfHost(this IServiceCollection services)
        {
            services.ResolveSettingsPackage();
            services.Configure<Configuration.Options>(options => options.GetSettings<Configuration.IOptions, Configuration.Options>("SelfHost"));

             //services.AddLettuceEncrypt().PersistDataToDirectory(new DirectoryInfo(Path.Combine(Zen.Base.Host.DataDirectory, "LettuceEncrypt")), "Password123");
        }
    }
}