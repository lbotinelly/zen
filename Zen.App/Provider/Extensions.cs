using System.Collections.Generic;
using System.Linq;
using Zen.Base.Extension;

namespace Zen.App.Provider
{
    public static class Extensions
    {
        public static IEnumerable<IZenGroup<T>> WithParents<T>(this IEnumerable<IZenGroup<T>> groups) where T : IZenPermission
        {
            var rawCollection = groups.Aggregate(new List<IZenGroup<T>>(), (i, j) =>
            {
                var a = Current.Orchestrator
                    .GetFullHierarchicalChain((IZenGroup<IZenPermission>) j)
                    .Select(k => (IZenGroup<T>) k).ToList();
                i.AddRange(a);
                return i;
            });

            return rawCollection.DistinctBy(i => i.Id).ToList();
        }

        public static List<T> GetPermissions<T>(this IEnumerable<IZenGroup<T>> groups) where T : IZenPermission
        {
            var rawCollection = groups.Aggregate(new List<T>(), (i, j) =>
            {
                var groupPermissions = j.Permissions;
                if (groupPermissions != null) i.AddRange(groupPermissions);

                return i;
            });

            return rawCollection.DistinctBy(i => i.Id).ToList();
        }
    }
}