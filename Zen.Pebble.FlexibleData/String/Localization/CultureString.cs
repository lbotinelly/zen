//using System.Collections.Generic;
//using System.Globalization;
//using System.Linq;
//using Zen.Pebble.FlexibleData.Common.Interface;
//using Zen.Pebble.FlexibleData.Culture;
//using Zen.Pebble.FlexibleData.String.Localization.Concrete;
//using Zen.Pebble.FlexibleData.String.Localization.Interface;

//namespace Zen.Pebble.FlexibleData.String.Localization
//{
//    public class CultureString : ICulturedVariantValue<string>
//    {

//        public Dictionary<string, IVariant<ICommentedTemporalValue<string>>> Variants { get; set; }
        
//        private CultureInfo _culture = CultureInfo.CurrentCulture;
//        public CultureString() { }
//        public CultureString(string source, string culture = null, string comment = null) => SetVariant(source, culture, comment);

//        public string Culture { get => _culture.Name; set => _culture = value.ToCultureInfo(); }

//        public string Value
//        {
//            get
//            {
//                // First case: Do we have any entries at all?
//                if (Variants == null) return null;
//                if (Variants.Count == 0) return null;

//                // DO we have a variant for the current culture? Otherwise pick whatever we have.
//                return Variants.ContainsKey(_culture.Name) ? Variants[_culture.Name].Variants.FirstOrDefault().Value : Variants.FirstOrDefault().Value.Variants.FirstOrDefault().Value;
//            }
//        }

//        public override string ToString()
//        {
//            string tmp = null;
//            if (Value != null) tmp = $"{Value}";

//            if (!(Variants?.Count > 0)) return tmp ?? base.ToString();

//            tmp += $" ({string.Join(", ", Variants.Select(i => $"[{i.Key}] {string.Join(", ", i.Value)}").ToList())})";

//            return tmp;
//        }

//        public CultureString SetVariant(string value, string culture = null, string comments = null)
//        {
//            if (string.IsNullOrEmpty(value?.Trim())) value = null;

//            value = value?.Trim();

//            if (Variants == null) Variants = new Dictionary<string, IVariant<ICommentedTemporalValue<string>>>();

//            culture = culture.ToCultureInfo()?.Name ?? _culture.Name;

//            if (value == null)
//            {
//                if (Variants.ContainsKey(culture)) Variants.Remove(culture);
//                return null;
//            }

//            Variants[culture] = comments != null ? new CommentedStringValue {Value = value, Comments = comments} : new StringValue {Value = value};

//            return this;
//        }

//        public void RemoveVariant(string culture)
//        {
//            culture = culture.ToCultureInfo()?.Name ?? _culture.Name;
//            if (Variants.ContainsKey(culture)) Variants.Remove(culture);
//        }

//        public static implicit operator CultureString(string source) { return new CultureString(source); }
//        public static implicit operator string(CultureString source) { return source.Value; }

//    }
//}