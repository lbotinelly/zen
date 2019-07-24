using System.Collections.Generic;
using System.Security.Principal;
using Zen.Base.Common;

namespace Zen.App.Provider
{
    public interface IAppOrchestrator<T> : IZenProvider where T: IZenPermission
    {
        IZenPerson<T> Person { get; }
        IZenApplication<T> Application { get; }
        object Settings { get; }
        IZenPerson<T> GetPersonByLocator(string locator);
        List<T> GetPermissionsByPerson(IZenPerson<T> person);
        IZenApplication<T> GetApplicationByLocator(string appLocator);
        IZenApplication<T> GetNewApplication();
        IZenApplication<T> UpsertApplication(IZenApplication<T> application);
        IZenPerson<T> SigninPersonByIdentity(IIdentity userIdentity);
        void SignInPerson(IZenPerson<T> person);
        IZenGroup<T> GetGroupByCode(string code);
        List<IZenGroup<T>> GetFullHierarchicalChain(IZenGroup<T> referenceGroup);
    }
}