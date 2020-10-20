using System.Collections.Generic;
using System.Linq;

namespace Zen.Base.Module
{
    public static class Extensions
    {
        public static Set<T> ToSet<T>(this IEnumerable<T> source) where T : Data<T>, ISetSave
        {
            return new Set<T> {Cache = source.ToDictionary(i => i.GetDataKey(), i => i)};
        }
    }
}