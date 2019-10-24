using System.Collections.Generic;
using Zen.App.Provider.Group;

namespace Zen.App.Provider
{
    public interface IZenGroup : IZenGroupBase
    {
        bool AddPermission(IZenPermission targetPermission);
        bool RemovePermission(IZenPermission targetPermission);
        bool AddPerson(IZenPerson person, bool automated = false, bool useNonAutomatedIfFound = false);
        bool RemovePerson(IZenPerson person, bool automated = false, bool useNonAutomatedIfFound = false);
        IEnumerable<IZenPerson> GetPeople();
    }
}