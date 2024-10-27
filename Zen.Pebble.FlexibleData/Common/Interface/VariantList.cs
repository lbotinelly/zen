using System.Collections.Generic;

namespace Zen.Pebble.FlexibleData.Common.Interface {
    public class VariantList<T> : IVariantList<T>
    {
        public List<T> Variants { get; set; }
    }
}