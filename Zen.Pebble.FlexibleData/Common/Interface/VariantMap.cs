using System.Collections.Generic;

namespace Zen.Pebble.FlexibleData.Common.Interface
{
    public class VariantMap<TU, T> : IVariantMap<TU, T>
    {
        public Dictionary<TU, VariantList<T>> Variants { get; set; }
    }
}