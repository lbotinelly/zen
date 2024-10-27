using System;
using Zen.Web.Auth.Handlers;
using Zen.Base.Module.Service;
using System.Linq;
using Zen.Base.Extension;

namespace Zen.Web.Auth
{
    public static class Current
    {
        public static IAuthEventHandler AuthEventHandler = IoC.GetClassesByInterface<IAuthEventHandler>(false).FirstOrDefault()?.CreateInstance<IAuthEventHandler>();
    }
}
