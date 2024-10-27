using System.Linq;

namespace Zen.Base.Module.Data.CommonAttributes
{
    public static class Extensions
    {
        public static T ByCode<T>(this T obj, string code, Mutator mutator = null) where T : Data<T>, IDataCode { return Data<T>.Where(i => i.Code == code, mutator).FirstOrDefault(); }

        public static T ByLocator<T>(this T obj, string locator, Mutator mutator = null) where T : Data<T>, IDataLocator { return Data<T>.Where(i => i.Locator == locator, mutator).FirstOrDefault(); }
    }
}