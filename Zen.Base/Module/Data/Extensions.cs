using System;
using System.Collections.Generic;
using System.Linq;
using Zen.Base.Extension;
using Zen.Base.Module.Data.CommonAttributes;

namespace Zen.Base.Module.Data
{
    public static class Extensions
    {
        public static Mutator ToModifier(this string source)
        {
            return source.IsJson() ? 
                new Mutator {Transform = new QueryTransform {Filter = source}} : 
                new Mutator {Transform = new QueryTransform {Statement = source}};
        }

        public static BulkDataOperation<T> Save<T>(this IEnumerable<T> modelSet) where T : Data<T> { return Data<T>.Save(modelSet); }

        public static BulkDataOperation<T> Remove<T>(this IEnumerable<T> modelSet) where T : Data<T>
        {
            return Data<T>.Remove(modelSet);
        }

        public static IEnumerable<T> AfterGet<T>(this IEnumerable<T> modelSet) where T : Data<T>
        {
            modelSet = modelSet.ToList();
            foreach (var model in modelSet) model?.AfterGet();
            return modelSet;
        }

        public static Dictionary<string, T> AsMap<T>(this IEnumerable<T> modelSet) where T : Data<T>, IDataId { return modelSet.ToDictionary(i => i.Id, i => i); }
        public static Dictionary<string, T> AsMap<T>(this IEnumerable<T> modelSet, Func<T, string> identifierFunction) { return modelSet.ToDictionary(identifierFunction, i => i); }
    }
}