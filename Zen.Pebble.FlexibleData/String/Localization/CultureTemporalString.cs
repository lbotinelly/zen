using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Zen.Pebble.FlexibleData.Common.Interface;
using Zen.Pebble.FlexibleData.Culture;
using Zen.Pebble.FlexibleData.String.Localization.Concrete;
using Zen.Pebble.FlexibleData.String.Localization.Interface;

namespace Zen.Pebble.FlexibleData.String.Localization
{
    public class CultureTemporalString : IVariantCultured<string>
    {
        private CultureInfo _culture = CultureInfo.CurrentCulture;
        public CultureTemporalString() { }

        public CultureTemporalString(string source, string culture = null, string comments = null)
        {
            _culture = culture.ToCultureInfo() ?? CultureInfo.CurrentCulture;
            SetVariant(source, culture, comments);
        }

        public string Culture { get => _culture.Name; set => _culture = value.ToCultureInfo(); }

        public string Value
        {
            get
            {
                // First case: Do we have any entries at all?
                if (Variants == null) return null;
                if (Variants.Count == 0) return null;

                // DO we have a variant for the current culture? Otherwise pick whatever we have.
                return Variants.ContainsKey(_culture.Name) ? Variants[_culture.Name].Variants.FirstOrDefault().Value : Variants.FirstOrDefault().Value.Variants.FirstOrDefault().Value;
            }
        }

        #region Implementation of ICulturedVariantValue<string>

        public Dictionary<string, IVariant<ITemporalCommented<string>>> Variants { get; set; }

        #endregion

        public override string ToString()
        {
            string tmp = null;
            if (Value != null) tmp = $"{Value}";

            if (!(Variants?.Count > 0)) return tmp ?? base.ToString();

            tmp += $" ({string.Join(", ", Variants.Select(i => $"[{i.Key}] {string.Join(", ", i.Value)}").ToList())})";

            return tmp;
        }

        public CultureTemporalString SetVariant(string value, string culture = null, string comments = null, System.DateTime? startDate = null, System.DateTime? endDate = null) { return SetVariant(value, culture, comments, null, startDate, endDate); }

        public CultureTemporalString SetVariant(string value, string culture = null, string comments = null) { return SetVariant(value, culture, comments, null); }

        public CultureTemporalString SetVariant(string value, string culture, string comments, string boundaryId, System.DateTime? startDate, System.DateTime? endDate)
        {
            if (value == null) return null;

            if (string.IsNullOrEmpty(value?.Trim())) value = null;

            value = value?.Trim();

            if (Variants == null) Variants = new Dictionary<string, IVariant<ITemporalCommented<string>>>();

            // Detect target Culture

            var cultureProbe = culture.ToCultureInfo()?.Name ?? _culture.Name;

            if (!Variants.ContainsKey(cultureProbe)) Variants[cultureProbe] = new VariantCommentedTemporalString { Variants = new List<ITemporalCommented<string>>() };

            // Detect target Boundary

            CommentedTemporalString targetEntry = null;

            if (boundaryId != null) { targetEntry = (CommentedTemporalString)Variants[cultureProbe].Variants.FirstOrDefault(i => i.Boundaries?.BoundaryId.Equals(boundaryId) == true); }
            else
            {
                targetEntry = new CommentedTemporalString(value, comments, startDate, endDate);
                Variants[cultureProbe].Variants.Add(targetEntry);
            }

            if (targetEntry == null) return this;

            targetEntry.Value = value;
            targetEntry.Comments = comments;

            if (startDate != null || endDate != null) targetEntry.Boundaries = new HistoricPrecisionBoundary(startDate, endDate);

            return this;
        }

        public static implicit operator CultureTemporalString(string source) { return new CultureTemporalString(source); }
        public static implicit operator string(CultureTemporalString source) { return source.Value; }
    }
}