using System.Collections.Generic;
using System.Security.Principal;
using Zen.Base.Common;
using static Zen.App.Orchestrator.Model.Application;

namespace Zen.App.Provider
{
    public interface IAppOrchestrator : IZenProvider
    {
        IZenPerson Person { get; }
        IZenApplication Application { get; }
        object Settings { get; }
        IZenPerson GetPersonByLocator(string locator);
        List<Permission> GetPermissionsByPerson(IZenPerson person);
        IZenApplication GetApplicationByLocator(string appLocator);
        IZenApplication GetNewApplication();
        IZenApplication UpsertApplication(IZenApplication application);
        IZenPerson SigninPersonByIdentity(IIdentity userIdentity);
        void SignInPerson(IZenPerson person);
        IZenGroup GetGroupByCode(string code);
        List<IZenGroup> GetFullHierarchicalChain(IZenGroup referenceGroup);
    }
}