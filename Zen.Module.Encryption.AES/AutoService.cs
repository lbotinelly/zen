using Microsoft.Extensions.DependencyInjection;
using Zen.Base;
using Zen.Base.Module.Service;

namespace Zen.Module.Encryption.AES
{
    public class AutoService : IZenAutoAddService
    {
        public void Add(IServiceCollection services)
        {
            services.Configure<AesEncryptionConfiguration.Options>(options => options.GetSettings<AesEncryptionConfiguration.IOptions, AesEncryptionConfiguration.Options>("Encryption:AES"));
        }
    }
}