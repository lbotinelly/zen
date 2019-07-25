using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Zen.Base.Module.Service
{
    public static class Instances
    {
        internal static ServiceDataBag ServiceData = new ServiceDataBag();
        private static IServiceCollection _serviceCollection;

        public static IServiceCollection ServiceCollection { get => _serviceCollection ?? (_serviceCollection = new ServiceCollection()); internal set => _serviceCollection = value; }
        public static ServiceProvider ServiceProvider { get; internal set; }
        public static IApplicationBuilder ApplicationBuilder { get; internal set; }

        internal class ServiceDataBag
        {
            public DateTime StartTimeStamp { get; set; }
            public DateTime? EndTimeStamp { get; set; }
            public TimeSpan UpTime => (EndTimeStamp ?? DateTime.Now) - StartTimeStamp;
        }
    }
}