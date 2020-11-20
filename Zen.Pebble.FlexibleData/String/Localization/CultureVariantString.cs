using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Zen.Pebble.FlexibleData.Common.Interface;
using Zen.Pebble.FlexibleData.Culture;

namespace Zen.Pebble.FlexibleData.String.Localization
{
    public class CultureVariantString : Dictionary<string, string>, IScoped<string, string>
    {
        private CultureInfo _culture = CultureInfo.CurrentCulture;

        public string Value
        {
            get
            {
                // First case: Do we have any entries at all?
                if (Count == 0) return null;

                // DO we have a variant for the current culture? Otherwise pick whatever we have.
                return ContainsKey(_culture.Name) ? this[_culture.Name] : this.FirstOrDefault().Value;
            }
            set
            {
                // Auto-resolved, so ignore. Setter preserved for serialization purposes.
            }
        }
        public string Scope { get => _culture.Name; set => _culture = value.ToCultureInfo(); }

        public CultureVariantString() {
            // No-parameter constructor for serialization purposes.
        }

        public CultureVariantString(string value) => SetVariant(value);

        public override string ToString()
        {
            string tmp = null;
            if (Value != null) tmp = $"{Value}";
            if (!(Count > 0)) return tmp ?? base.ToString();
            return tmp;
        }

        public CultureVariantString SetVariant(string value, string culture = null)
        {
            if (value == null) return null;

            if (string.IsNullOrEmpty(value?.Trim())) value = null;

            value = value?.Trim();


            // Detect target Culture

            var cultureProbe = culture.ToCultureInfo()?.Name ?? _culture.Name;

            this[cultureProbe] = value;

            return this;
        }
        public static implicit operator CultureVariantString(string source) => string.IsNullOrEmpty(source) ? null : new CultureVariantString(source);
        public static implicit operator string(CultureVariantString source) => source.Value;
    }
}