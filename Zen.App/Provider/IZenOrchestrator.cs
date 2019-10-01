using System.Collections.Generic;
using System.Security.Principal;
using Zen.App.Provider.Application;
using Zen.App.Provider.Person;
using Zen.Base.Common;

namespace Zen.App.Provider
{
    public interface IZenOrchestrator : IZenProvider
    {
        IZenPerson Person { get; }
        IZenApplication Application { get; }
        Dictionary<string, object> Settings { get; }
        IZenPerson GetPersonByLocator(string locator);
        IEnumerable<IZenPerson> GetPeopleByLocators(IEnumerable<string> locators);
        List<IZenPermission> GetPermissionsByPerson(IZenPerson person);
        IZenApplication GetApplicationByLocator(string appLocator);
        IZenApplication GetNewApplication();
        IZenApplication UpsertApplication(IZenApplication application);
        IZenPerson SigninPersonByIdentity(IIdentity userIdentity);
        void SignInPerson(IZenPerson person);
        IZenGroup GetGroupByCode(string code, string name = null, IZenApplication application = null, IZenGroup parent = null, bool createIfNotFound = false);
        List<IZenGroup> GetFullHierarchicalChain(IZenGroup referenceGroup, bool ignoreParentWhenAppOwned = true);
        List<IZenGroup> GroupsByApplication(string key);
        bool HasAnyPermissions(string expression);
        bool HasAnyPermissions(IEnumerable<string> terms);
        IZenPermission GetPermissionByCode(string code, string name = null, IZenApplication application = null, bool createIfNotFound = false);
        List<IZenPerson> GetPeople(IEnumerable<string> keys = null);
        void SavePerson(List<IZenPerson> people);
        string GetApiUri();
        string GetResourceUri();
        List<IZenPerson> PeopleByGroup(string key);
        List<IZenPersonProfile> GetProfiles(string keys);
        IZenPersonProfile GetProfile(IZenPerson person);
        IZenApplication GetApplicationById(string identifier);
        IZenPerson GetPersonByEmail(string email);
    }
}