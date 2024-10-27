using System.Collections.Generic;
using System.Linq;
using Zen.Base.Extension;
using Zen.Base.Module.Service;

namespace Zen.App.Core.Application
{
    public static class Factory
    {
        private static readonly List<IApplicationProvider> Providers = IoC.GetClassesByInterface<IApplicationProvider>(false).CreateInstances<IApplicationProvider>().ToList();
        private static IApplicationProvider Provider => Providers.FirstOrDefault(p => p.Application!= null);
        public static IApplication Current => Provider?.Application;
        public static IApplication Compile() => Provider?.Compile(true);
    }
}