using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Zen.Pebble.FlexibleData.DateTime
{
    public static class Calendars
    {
        public enum ECalendarType
        {
            Gregorian,
            Hebrew,
            Hijri,
            Japanese,
            JapaneseLunisolar,
            Julian,
            Korean,
            KoreanLunisolar,
            Persian,
            Taiwan,
            TaiwanLunisolar,
            ThaiBuddhist,
            UmAlQura
        }

        public static Dictionary<ECalendarType, Calendar> Map = new Dictionary<ECalendarType, Calendar>();
        public static Dictionary<ECalendarType, List<string>> CulturesPerCalendar = new Dictionary<ECalendarType, List<string>>();

        static Calendars()
        {
            // Cache the default Calendar Implementations
            Map[ECalendarType.Gregorian] = new GregorianCalendar();
            Map[ECalendarType.Hebrew] = new HebrewCalendar();
            Map[ECalendarType.Hijri] = new HijriCalendar();
            Map[ECalendarType.Japanese] = new JapaneseCalendar();
            Map[ECalendarType.JapaneseLunisolar] = new JapaneseLunisolarCalendar();
            Map[ECalendarType.Julian] = new JulianCalendar();
            Map[ECalendarType.Korean] = new KoreanCalendar();
            Map[ECalendarType.KoreanLunisolar] = new KoreanLunisolarCalendar();
            Map[ECalendarType.Persian] = new PersianCalendar();
            Map[ECalendarType.Taiwan] = new TaiwanCalendar();
            Map[ECalendarType.TaiwanLunisolar] = new TaiwanLunisolarCalendar();
            Map[ECalendarType.ThaiBuddhist] = new ThaiBuddhistCalendar();
            Map[ECalendarType.UmAlQura] = new UmAlQuraCalendar();

            // Prepare the CulturesPerCalendar map
            var calendarCultureMap = new Dictionary<string, List<string>>();

            foreach (var culture in CultureInfo.GetCultures(CultureTypes.AllCultures))
            {
                // Cultures have one or more Calendars - the main one, and the optionals. Let's grab'em all
                var allCultureCalendars = new List<Calendar> {culture.Calendar};
                allCultureCalendars.AddRange(culture.OptionalCalendars.ToList());

                // Add the culture to calendarCultureMap.
                foreach (var calendarName in allCultureCalendars.Select(c => c.GetType().Name))
                {
                    if (!calendarCultureMap.ContainsKey(calendarName)) calendarCultureMap[calendarName] = new List<string>();

                    //Some cultures have no code. Who would knew!
                    if (string.IsNullOrEmpty(culture.Name)) continue;

                    if (!calendarCultureMap[calendarName].Contains(culture.Name)) calendarCultureMap[calendarName].Add(culture.Name);
                }
            }

            foreach (var (key, value) in calendarCultureMap) CulturesPerCalendar[Map.FirstOrDefault(i => i.Value.GetType().Name == key).Key] = value;
        }
    }
}