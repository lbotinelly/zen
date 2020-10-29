using System.Collections.Generic;

namespace Zen.Pebble.FlexibleData.Common.Interface {
    public class Variant<T> : IVariant<T>
    {
        public List<T> Variants { get; set; }
    }
}