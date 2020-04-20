using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Zen.Base;
using Zen.Base.Extension;
using Zen.Base.Module.Service;

namespace Zen.Module.Encryption.AES
{
    public class AutoService : IZenAutoAddService
    {
        public void Add(IServiceCollection services)
        {
            services.Configure<AesEncryptionConfiguration.Options>(options =>
            {
                var configOptions =
                    IoC.GetClassesByInterface<AesEncryptionConfiguration.IOptions>().FirstOrDefault()?.ToInstance<AesEncryptionConfiguration.IOptions>() ??
                    Configuration.Options.GetSection("Encryption:AES").Get<AesEncryptionConfiguration.Options>();

                options.Key = configOptions.Key;
                options.InitializationVector = configOptions.InitializationVector;
            });
        }
    }
}