using System;
using System.Collections.Concurrent;
using Zen.Pebble.Database.Common;

namespace Zen.Pebble.Database.Cache
{
    public static class CachedInstances
    {
        public static ConcurrentDictionary<Type, ModelDescriptor> ModelDescriptors = new ConcurrentDictionary<Type, ModelDescriptor>();
    }
}