using System.Collections.Generic;
using System.Linq;
using Zen.Base.Extension;
using Zen.Base.Module.Service;

namespace Zen.App.Core.Person
{
    public class Factory
    {
        private static readonly List<IPersonProvider> Providers = IoC.GetClassesByInterface<IPersonProvider>(false).CreateInstances<IPersonProvider>().ToList();
        public static IPerson Current => Providers.Select(zenPersonProvider => zenPersonProvider.Person).FirstOrDefault(p => p!= null);
    }
}