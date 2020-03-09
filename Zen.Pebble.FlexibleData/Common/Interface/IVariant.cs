using System.Collections.Generic;
using Zen.Pebble.FlexibleData.String.Localization.Interface;

namespace Zen.Pebble.FlexibleData.Common.Interface
{
    public interface IVariant<T>
    {
        List<T> Variants { get; set; }
    }

    public class Variant<T>: IVariant<T> {
        public List<T> Variants { get; set; }
    }
}