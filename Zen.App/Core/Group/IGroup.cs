using System.Collections.Generic;
using Zen.App.Core.Application;
using Zen.App.Core.Person;
using Zen.App.Provider;

namespace Zen.App.Core.Group
{
    public interface IGroup : IGroupBase
    {
        bool AddPermission(IPermission targetPermission);
        bool RemovePermission(IPermission targetPermission);
        bool AddPerson(IPerson person, bool automated = false, bool useNonAutomatedIfFound = false);
        bool RemovePerson(IPerson person, bool automated = false, bool useNonAutomatedIfFound = false);
        IEnumerable<IPerson> GetPeople();
    }
}