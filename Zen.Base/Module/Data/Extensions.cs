using System.Collections.Generic;

namespace Zen.Base.Module.Data
{
    public static class Extensions
    {
        public static Mutator ToModifier(this string source) { return new Mutator {Transform = new QueryTransform {Filter = source}}; }

        public static BulkDataOperation<T> Save<T>(this IEnumerable<T> modelSet) where T : Data<T> { return Data<T>.Save(modelSet); }

        public static BulkDataOperation<T> Remove<T>(this IEnumerable<T> modelSet) where T : Data<T> { return Data<T>.Remove(modelSet); }
    }
}