using System.Collections.Generic;
using System.Linq;
using Zen.Base.Extension;
using static Zen.App.Orchestrator.Model.Application;

namespace Zen.App.Provider
{
    public static class Extensions
    {
        public static IEnumerable<IZenGroup> WithParents(this IEnumerable<IZenGroup> groups)
        {
            var rawCollection = groups.Aggregate(new List<IZenGroup>(), (i, j) =>
            {
                var a = Current.Orchestrator
                    .GetFullHierarchicalChain(j)
                    .ToList();
                i.AddRange(a);
                return i;
            });

            return rawCollection.DistinctBy(i => i.Id).ToList();
        }

        public static List<Permission> GetPermissions(this IEnumerable<IZenGroup> groups)
        {
            var rawCollection = groups.Aggregate(new List<Permission>(), (i, j) =>
            {
                var groupPermissions = j.Permissions;
                if (groupPermissions != null) i.AddRange(groupPermissions);

                return i;
            });

            return rawCollection.DistinctBy(i => i.Id).ToList();
        }
    }
}