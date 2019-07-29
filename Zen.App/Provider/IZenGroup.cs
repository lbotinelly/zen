using Zen.App.Provider.Group;

namespace Zen.App.Provider
{
    public interface IZenGroup : IZenGroupBase
    {
        void AddPermission(IZenPermission targetPermission);
        void RemovePermission(IZenPermission targetPermission);
        void AddPerson(IZenPerson person, bool automated = false, bool useNonAutomatedIfFound = false);
        void RemovePerson(IZenPerson person, bool automated = false, bool useNonAutomatedIfFound = false);
    }
}