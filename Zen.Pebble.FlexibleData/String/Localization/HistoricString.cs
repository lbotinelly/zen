using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Zen.Pebble.FlexibleData.Common.Interface;
using Zen.Pebble.FlexibleData.Culture;
using Zen.Pebble.FlexibleData.Historical;
using Zen.Pebble.FlexibleData.String.Localization.Concrete;
using Zen.Pebble.FlexibleData.String.Localization.Interface;

namespace Zen.Pebble.FlexibleData.String.Localization
{
    public class HistoricString : VariantTemporalMap<string, string>, IScoped<string, string>
    {
        private CultureInfo _culture = CultureInfo.CurrentCulture;
        public HistoricString() { }

        public HistoricString(string source, string culture = null, string comments = null)
        {
            _culture = culture.ToCultureInfo() ?? CultureInfo.CurrentCulture;
            SetVariant(source, culture, comments);
        }

        public string Value
        {
            get
            {
                // First case: Do we have any entries at all?
                if (Variants == null) return null;
                if (Variants.Count == 0) return null;

                // DO we have a variant for the current culture? Otherwise pick whatever we have.
                return Variants.ContainsKey(_culture.Name) ? Variants[_culture.Name].Variants.FirstOrDefault()?.Value : Variants.FirstOrDefault().Value.Variants.FirstOrDefault()?.Value;
            }
            set
            {
                // Auto-resolved, so ignore. Setter preserved for serialization purposes.
            }
        }
        public string Scope { get => _culture.Name; set => _culture = value.ToCultureInfo(); }

        public override string ToString()
        {
            string tmp = null;
            if (Value != null) tmp = $"{Value}";

            if (!(Variants?.Count > 0)) return tmp ?? base.ToString();

            // tmp += $" ({string.Join(", ", Variants.Select(i => $"[{i.Key}] {string.Join(", ", i.Value)}").ToList())})";

            return tmp;
        }

        public HistoricString SetVariant(string value, string culture = null, string comments = null, string periodId = null, string startDate = null, string endDate = null)
        {
            if (value == null) return null;

            if (string.IsNullOrEmpty(value?.Trim())) value = null;

            value = value?.Trim();

            if (Variants == null) Variants = new Dictionary<string, VariantList<TemporalCommented<string>>>();

            // Detect target Culture

            var cultureProbe = culture.ToCultureInfo()?.Name ?? _culture.Name;

            if (!Variants.ContainsKey(cultureProbe)) Variants[cultureProbe] = new HistoricCultureString {Variants = new List<TemporalCommented<string>>()};

            // Detect target Boundary

            TemporalCommented<string> targetEntry = null;

            if (periodId != null) { targetEntry = Variants[cultureProbe].Variants.FirstOrDefault(i => i.Period?.PeriodId.Equals(periodId) == true); }
            else
            {
                targetEntry = Variants[cultureProbe].Variants.FirstOrDefault(i => i.Value?.Equals(value) == true);

                if (targetEntry == null)
                {
                    targetEntry = new TemporalCommented<string> {Comments = comments, Value = value, Period = 
                        startDate == null && endDate == null ? 
                            null: 
                            new HistoricPeriod(startDate, endDate)};
                    Variants[cultureProbe].Variants.Add(targetEntry);
                }
            }

            if (targetEntry == null) return this;

            targetEntry.Value = value;
            targetEntry.Comments = comments;

            if (startDate != null || endDate != null) targetEntry.Period = new HistoricPeriod(startDate, endDate);

            return this;
        }

        public static implicit operator HistoricString(string source) { return string.IsNullOrEmpty(source) ? null : new HistoricString(source); }

        public static implicit operator string(HistoricString source) { return source.Value; }
    }
}