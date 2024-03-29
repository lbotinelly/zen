﻿using Microsoft.Extensions.DependencyInjection;
using Zen.Base.Common;
using Zen.Base.Module.Service;
using Zen.Provider.OpenId.Authentication;

namespace Zen.Provider.OpenId
{
    [Priority(Level = -99)]
    public class AutoService : IZenAutoAddService
    {
        public void Add(IServiceCollection services)
        {
            services.Configure();
        }
    }
}
