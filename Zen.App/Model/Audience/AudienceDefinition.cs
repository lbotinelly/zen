using System.Collections.Generic;
using System.Linq;

namespace Zen.App.Model.Audience
{
    public class AudienceDefinition
    {
        public List<string> Groups = new List<string>();
        public List<string> People = new List<string>();
        public List<string> Permissions = new List<string>();
        public List<string> Tags = new List<string>();

        public bool IsVisibleToCurrentPerson() { return Current.Orchestrator.Person!= null && IsVisibleToPerson(Current.Orchestrator.Person?.Locator); }

        public bool IsVisibleToPerson(string locator)
        {
            if (locator == null) return false;

            if (Groups.Count == 0 && (People.Count == 0) & (Permissions.Count == 0)) return true;

            var grps = Current.Orchestrator
                .GetPersonByLocator(locator)
                .Groups().Select(i => i.Code);

            if (Groups.Count > 0)
                if (grps.Any(i => Groups.Contains(i)))
                    return true;

            var person = Current.Orchestrator.GetPersonByLocator(locator);

            if (Permissions.Count > 0)
                if (person.HasAnyPermissions(Permissions.Aggregate((i, j) => i + "," + j)))
                    return true;

            if (People.Count > 0)
                if (People.Contains(person.Locator))
                    return true;

            return false;
        }
    }
}