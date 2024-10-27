using System.Collections.Generic;
using System.Linq;
using Zen.Base.Module.Data.CommonAttributes;

namespace Zen.Base.Module
{
    public static class Extensions
    {
        public static Set<T> ToSet<T>(this IEnumerable<T> source) where T : Data<T>, ISetSave, IDataId
        {
            return new Set<T> {Cache = source.ToDictionary(i => i.GetDataKey(), i => i)};
        }
    }
}