using System.Linq;
using Zen.App.Provider;
using Zen.App.Provider.Application;
using Zen.App.Provider.Group;
using Zen.App.Provider.Person;
using Zen.Base.Extension;

namespace Zen.App.Orchestrator.Model
{
    public static class Extensions
    {
        public static ZenApplicationRepresentation ToRepresentation(this IZenApplication source)
        {
            var output = source.ToJson().FromJson<ZenApplicationRepresentation>();

            var groups = Current.Orchestrator.GroupsByApplication(output.Id);

            output.Groups = groups.Select(i =>
            {
                var groupAction = i.ToJson().FromJson<ZenGroupAction>();

                groupAction.People = Current.Orchestrator.PeopleByGroup(groupAction.Id).Select(j => j.ToJson().FromJson<ZenPersonAction>()).ToList();

                return groupAction;
            }).ToList();

            return output;
        }
    }
}