using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System;
using Zen.Base.Common;
using Zen.Base.Extension;
using Zen.Base.Module.Service;

namespace Zen.Web
{
    public class Configuration : IConfigureOptions<Configuration.IOptions>
    {
        public enum EMode
        {
            Auto,
            Custom
        }

        private readonly IOptions _options;

        public Configuration(IOptions<Options> options) => _options = options.Value;

        public void Configure(IOptions options)
        {
            _options.CopyMembersTo(options);
        }

        public interface IOptions
        {
            string WebQualifiedServerName { get; set; }
            int HttpPort { get; set; }
            int HttpsPort { get; set; }
            public bool UseIisIntegration { get; set; }
            public BehaviorDescriptor Behavior { get; set; }
            public string CertificateSubject { get; set; }
            public string CertificateFile { get; set; }
            public string CertificatePassword { get; set; }
            public string QualifiedServerName { get; set; }
            public string RoutePrefix { get; set; }
            public string SourcePath { get; set; }
            public bool EnableHtml5 { get; set; }
            public bool EnableSpa { get; set; }
            public bool EnableHsts { get; set; }
            public bool EnableHttpsRedirection { get; set; }
            public PathString DefaultPage { get; set; }
        }

        [IoCIgnore]
        public class Options : AutoOptions { }

        [Priority(Level = -99)]
        public class AutoOptions : IOptions // If nothing else is defined, AutoOptions kicks in.
        {
            public string WebQualifiedServerName { get; set; } = null;
            public int HttpPort { get; set; } = 5000;
            public int HttpsPort { get; set; } = 5001;
            public bool UseIisIntegration { get; set; } = false;
            public BehaviorDescriptor Behavior { get; set; } = new BehaviorDescriptor();
            public string CertificateSubject { get; set; }
            public string CertificateFile { get; set; }
            public string CertificatePassword { get; set; }
            public string QualifiedServerName { get; set; }
            public string RoutePrefix { get; set; }
            public string SourcePath { get; set; }
            public bool EnableHtml5 { get; set; }
            public bool EnableSpa { get; set; } = true;
            public bool EnableHsts { get; set; } = true;
            public bool EnableHttpsRedirection { get; set; } = true;
            public PathString DefaultPage
            {
                get => _defaultPage;
                set
                {
                    if (string.IsNullOrEmpty(value.Value)) throw new ArgumentException($"The value for {nameof(DefaultPage)} cannot be null or empty.");

                    _defaultPage = value;
                }
            }


            private PathString _defaultPage = "/index.html";

        }

        public class BehaviorDescriptor
        {
            public bool UseAppCodeAsRoutePrefix { get; set; } = false;
        }

    }
}