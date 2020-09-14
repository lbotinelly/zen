using Microsoft.Extensions.Options;
using Zen.Base.Common;
using Zen.Base.Module.Service;

namespace Zen.Web.GraphQL
{
    public class Configuration : IConfigureOptions<Configuration.Options>
    {
        public enum ETypeNameResolution
        {
            Name,
            FullName
        }

        private readonly IOptions _options;

        public Configuration(IOptions<Options> options)
        {
            _options = options.Value;
        }

        public void Configure(Options options)
        {
            options.TypeNameResolution = _options.TypeNameResolution;
        }

        public interface IOptions
        {
            ETypeNameResolution TypeNameResolution { get; set; }
        }

        [IoCIgnore]
        public class Options : IOptions
        {
            public ETypeNameResolution TypeNameResolution { get; set; }
        }

        [Priority(Level = -99)]
        public class AutoOptions : IOptions // If nothing else is defined, AutoOptions kicks in.
        {
            public ETypeNameResolution TypeNameResolution { get; set; } = ETypeNameResolution.Name;
        }
    }
}