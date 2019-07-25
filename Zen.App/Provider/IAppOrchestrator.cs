using System.Collections.Generic;
using System.Security.Principal;
using Zen.Base.Common;

namespace Zen.App.Provider
{
    public interface IAppOrchestrator : IZenProvider
    {
        IZenPerson Person { get; }
        IZenApplication Application { get; }
        object Settings { get; }
        IZenPerson GetPersonByLocator(string locator);
        List<IZenPermission> GetPermissionsByPerson(IZenPerson person);
        IZenApplication GetApplicationByLocator(string appLocator);
        IZenApplication GetNewApplication();
        IZenApplication UpsertApplication(IZenApplication application);
        IZenPerson SigninPersonByIdentity(IIdentity userIdentity);
        void SignInPerson(IZenPerson person);
        IZenGroup GetGroupByCode(string code);
        List<IZenGroup> GetFullHierarchicalChain(IZenGroup referenceGroup);
        bool HasAnyPermissions(string expression);
        bool HasAnyPermissions(IEnumerable<string> terms);
        IZenPermission GetPermissionByFullCode(string fullCode);
        List<IZenPerson> GetAllPeople();
        void SavePerson(List<IZenPerson> people);
    }
}