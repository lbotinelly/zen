using System.Collections.Generic;

namespace Zen.Pebble.FlexibleData.Common.Interface
{
    public interface IVariant<T>
    {
        List<T> Variants { get; set; }
    }
}