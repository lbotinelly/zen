using System.Collections.Generic;
using System.Security.Principal;
using Zen.App.Core.Application;
using Zen.App.Core.Group;
using Zen.App.Core.Person;
using Zen.Base.Common;

namespace Zen.App.Provider
{
    public interface IZenOrchestrator : IZenProvider
    {
        IPerson Person { get; }
        IApplication Application { get; }
        Dictionary<string, object> Settings { get; }
        IPerson GetPersonByLocator(string locator);
        IEnumerable<IPerson> GetPeopleByLocators(IEnumerable<string> locators);
        List<IPermission> GetPermissionsByPerson(IPerson person);
        IApplication GetApplicationByCode(string code);
        IApplication GetApplicationByLocator(string appLocator);
        IApplication GetNewApplication();
        IApplication UpsertApplication(IApplication application);
        IPerson SigninPersonByIdentity(IIdentity userIdentity);
        void SignInPerson(IPerson person);
        IGroup GetGroupByCode(string code, string name = null, IApplication application = null, IGroup parent = null, bool createIfNotFound = false);
        List<IGroup> GetFullHierarchicalChain(IGroup referenceGroup, bool ignoreParentWhenAppOwned = true);
        List<IGroup> GroupsByApplication(string key);
        bool HasAnyPermissions(string expression);
        bool HasAnyPermissions(IEnumerable<string> terms);
        IPermission GetPermissionByCode(string code, string name = null, IApplication application = null, bool createIfNotFound = false);
        List<IPerson> GetPeople(IEnumerable<string> keys = null);
        void SavePerson(List<IPerson> people);
        string GetApiUri();
        string GetResourceUri();
        List<IPerson> PeopleByGroup(string key);
        List<IPersonProfile> GetProfiles(string keys);
        IPersonProfile GetProfile(IPerson person);
        IApplication GetApplicationById(string identifier);
        IPerson GetPersonByEmail(string email);
        IPerson GetPersonByClaims(Dictionary<string, string> modelClaims);
    }
}