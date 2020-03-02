using System.Globalization;

namespace Zen.Pebble.FlexibleData.Culture
{
    public static class Extensions
    {
        public static CultureInfo ToCultureInfo(this string code) { return code == null ? null : CultureInfo.GetCultureInfo(code); }
    }
}