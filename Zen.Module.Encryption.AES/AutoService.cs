using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Zen.Base.Module.Service;

namespace Zen.Module.Encryption.AES
{
    public class AutoService : IZenAutoAddService
    {
        public void Add(IServiceCollection services)
        {
            services.Configure<AesEncryptionOptions>(options =>
            {
                var configOptions = 

                    Base.Configuration.Options.GetSection("Encryption:AES").Get<AesEncryptionOptions>();

                options.Key = configOptions.Key;
                options.InitializationVector = configOptions.InitializationVector;
            });
        }
    }
}