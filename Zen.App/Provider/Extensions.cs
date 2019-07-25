using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Zen.Base.Extension;
using Zen.Base.Module.Log;
using static Zen.App.Orchestrator.Model.Application;

namespace Zen.App.Provider
{
    public static class Extensions
    {
        public static IEnumerable<IZenGroup> WithParents(this IEnumerable<IZenGroup> groups)
        {
            var rawCollection = groups.Aggregate(new List<IZenGroup>(), (i, j) =>
            {
                i.AddRange(Current.Orchestrator.GetFullHierarchicalChain(j).ToList());
                return i;
            });

            return rawCollection.DistinctBy(i => i.Id).ToList();
        }

        public static List<Permission> GetPermissions(this IEnumerable<IZenGroup> groups)
        {
            var rawKeyCollection = groups.Aggregate(new List<string>(), (i, j) =>
            {
                var groupPermissions = j.Permissions;
                if (groupPermissions != null) i.AddRange(groupPermissions);

                return i;
            }).Distinct();

            return Permission.Get(rawKeyCollection).ToList();
        }

        public static void CompileAllPeoplePermissions(this IAppOrchestrator source)
        {
            var people = source.GetAllPeople();

            var c = new Clicker("Compiling Person permissions", people);


            Parallel.ForEach(people, new ParallelOptions { MaxDegreeOfParallelism = 10 }, zenPerson =>
            { 
                c.Click();
                zenPerson.Permissions = source.GetPermissionsByPerson(zenPerson).Select(i => i.FullCode).ToList();
            });

            c.End();

            source.SavePerson(people);


        }
    }
}