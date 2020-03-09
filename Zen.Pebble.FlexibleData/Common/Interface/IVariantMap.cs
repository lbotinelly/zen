using System.Collections.Generic;

namespace Zen.Pebble.FlexibleData.Common.Interface
{
    public interface IVariantMap<TU, T>
    {
        Dictionary<TU, Variant<T>> Variants { get; set; }
    }

    public class VariantMap<TU, T> : IVariantMap<TU, T>
    {
        public Dictionary<TU, Variant<T>> Variants { get; set; }
    }
}