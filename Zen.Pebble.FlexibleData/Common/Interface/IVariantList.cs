using System.Collections.Generic;

namespace Zen.Pebble.FlexibleData.Common.Interface
{
    public interface IVariantList<T> 
    {
        List<T> Variants { get; set; }
    }
}