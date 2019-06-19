using System.Collections.Generic;
using Zen.Base.Module.Data.Adapter;

namespace Zen.Base.Module.Data
{
    public static class Extensions
    {
        public static QueryModifier ToModifier(this string source) => new QueryModifier { Payload = new QueryPayload { OmniQuery = source } };
        public static BulkDataOperation<T> Save<T>(this IEnumerable<T> modelSet) where T : Data<T> => Data<T>.Save(modelSet);
        public static BulkDataOperation<T> Remove<T>(this IEnumerable<T> modelSet) where T : Data<T> => Data<T>.Remove(modelSet);
    }
}