using System;
using Microsoft.AspNetCore.Http;

namespace Zen.Module.Web.REST.Startup
{
    public class ZenWebOptions
    {
        private PathString _defaultPage = "/index.html";
        public ZenWebOptions() { }

        internal ZenWebOptions(ZenWebOptions copyFromOptions)
        {
            _defaultPage = copyFromOptions.DefaultPage;
            SourcePath = copyFromOptions.SourcePath;
        }

        public PathString DefaultPage
        {
            get => _defaultPage;
            set
            {
                if (string.IsNullOrEmpty(value.Value)) throw new ArgumentException($"The value for {nameof(DefaultPage)} cannot be null or empty.");

                _defaultPage = value;
            }
        }

        public string SourcePath { get; set; }
        public bool UseHtml5 { get; set; }
    }
}