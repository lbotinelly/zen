using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Zen.Pebble.FlexibleData.Common.Interface;

namespace Zen.Pebble.FlexibleData.DateTime
{
    public class VariantCalendarDateTime : IValue<System.DateTime>
    {
        private Calendar _calendar = new GregorianCalendar();
        private CultureInfo _culture = CultureInfo.CurrentCulture;
        private System.DateTime _value = System.DateTime.MinValue;
        private Dictionary<Calendars.ECalendarType, string> _variants;

        public VariantCalendarDateTime(string dateString = null, string culture = null, string calendar = null)
        {
            SetCultureByString(culture);
            SetCalendarByString(calendar);
            SetValueByString(dateString);
        }

        public VariantCalendarDateTime(System.DateTime date, CultureInfo culture)
        {
            Value = date;
            Culture = culture;
        }

        public CultureInfo Culture
        {
            get => _culture;
            set
            {
                _culture = value;
                ClearVariantsCache();
            }
        }

        public Dictionary<Calendars.ECalendarType, string> Variants
        {
            get
            {
                CompileVariants();
                return _variants;
            }
        }

        #region Implementation of IValue<DateTime>

        public System.DateTime Value
        {
            get => _value;
            set
            {
                _value = value;
                ClearVariantsCache();
            }
        }

        #endregion

        private bool SetCalendarByString(string calendarString)
        {
            if (calendarString == null) return true;

            try
            {
                var probeECalendarType = (Calendars.ECalendarType) Enum.Parse(typeof(Calendars.ECalendarType), calendarString);
                _calendar = Calendars.Map[probeECalendarType];
                return true;
            } catch (Exception) { return false; }
        }

        private bool SetValueByString(string valueString)
        {
            if (valueString == null) return true;

            try
            {
                var entryBuffer = System.DateTime.Parse(valueString);
                var valueBuffer = entryBuffer.FromCalendar(_calendar);
                if (valueBuffer.HasValue) _value = valueBuffer.Value;
                return true;
            } catch (Exception) { return false; }
        }

        private void SetCultureByString(string cultureString)
        {
            if (cultureString == null) return;
            Culture = CultureInfo.GetCultureInfo(cultureString);
            _calendar = Culture.Calendar;
        }

        private void ClearVariantsCache() { _variants = null; }

        private void CompileVariants()
        {
            if (_variants!= null) return;

            _variants = new Dictionary<Calendars.ECalendarType, string>();

            // First, determine the source culture prefix (i.e. the language)

            var languageParticle = Culture.Name.Split('-')[0] + "-";

            foreach (var (key, value) in Calendars.Map)
                // Locate a Culture that handles the same language, or default to first/preferred.
            {
                var outputFormat = Math.Abs(_value.TimeOfDay.TotalSeconds) > 0 ? "F" : "D"; // Full for values with time part, date-only otherwise

                string dateLocalizedExpression = null;

                if (Calendars.CulturesPerCalendar.ContainsKey(key))
                {
                    var targetCultureName = Calendars.CulturesPerCalendar[key].FirstOrDefault(i => i.StartsWith(languageParticle)) ?? Calendars.CulturesPerCalendar[key].FirstOrDefault();

                    if (targetCultureName!= null)
                    {
                        var targetCulture = new CultureInfo(targetCultureName) {DateTimeFormat = {Calendar = value}};

                        try { dateLocalizedExpression = _value.ToString(outputFormat, targetCulture); } catch (Exception) { }
                    }
                }

                if (dateLocalizedExpression == null) dateLocalizedExpression = _value.ToCalendar(value)?.ToString(outputFormat, _culture);

                if (dateLocalizedExpression!= null) _variants[key] = dateLocalizedExpression;
            }
        }
    }
}