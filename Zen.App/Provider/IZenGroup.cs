using System.Collections.Generic;
using Zen.Base.Module.Data.CommonAttributes;

namespace Zen.App.Provider
{
    public interface IZenGroup : IDataId, IDataCode, IDataActive 
    {
        string Name { get; set; }
        List<string> Permissions { get; set; }
        bool FromSettings { get; set; }
        string ParentId { get; set; }
        string ApplicationId { get; set; }
        void AddPermission(IZenPermission targetPermission);
        void RemovePermission(IZenPermission targetPermission);
        void AddPerson(IZenPerson person, bool automated = false, bool useNonAutomatedIfFound = false);
        void RemovePerson(IZenPerson person, bool automated = false, bool useNonAutomatedIfFound = false);
    }
}