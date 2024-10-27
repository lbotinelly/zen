using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Zen.App.Core.Group;
using Zen.App.Model.Core;
using Zen.Base.Extension;
using Zen.Base.Module.Log;

namespace Zen.App.Provider
{
    public static class Extensions
    {
        public static IEnumerable<IGroup> WithParents(this IEnumerable<IGroup> groups)
        {
            var rawCollection = groups.Aggregate(new List<IGroup>(), (i, j) =>
            {
                i.AddRange(Current.Orchestrator.GetFullHierarchicalChain(j).ToList());
                return i;
            });

            return rawCollection.DistinctBy(i => i.Id).ToList();
        }

        public static List<Permission> GetPermissions(this IEnumerable<IGroup> groups)
        {
            var rawKeyCollection = groups.Aggregate(new List<string>(), (i, j) =>
            {
                var groupPermissions = j.Permissions;
                if (groupPermissions!= null) i.AddRange(groupPermissions);

                return i;
            }).Distinct();

            return Permission.Get(rawKeyCollection).ToList();
        }

        public static void CompileAllPeoplePermissions(this IZenOrchestrator source)
        {
            var people = source.GetPeople();

            var c = new Clicker("Compiling Person permissions", people);

            Parallel.ForEach(people, new ParallelOptions {MaxDegreeOfParallelism = 10}, zenPerson =>
            {
                c.Click();
                zenPerson.Permissions = source.GetPermissionsByPerson(zenPerson).Select(i => i.FullCode).ToList();
            });

            c.End();

            source.SavePerson(people);
        }
    }
}