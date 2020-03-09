using System.Collections.Generic;
using System.Linq;
using Zen.Pebble.FlexibleData.Common.Interface;
using Zen.Pebble.FlexibleData.String.Localization.Interface;

namespace Zen.Pebble.FlexibleData.Historical
{
    public class HistoricCultureString : Variant<TemporalCommented<string>>
    {
        public override string ToString() => Variants.FirstOrDefault()?.Value;
    }
}