using System.Security.Principal;
using Zen.Base.Common;

namespace Zen.Web.Auth.Handlers
{
    public interface IAuthEventHandler : IZenProvider
    {
        IIdentity OnConfirmSignIn(IIdentity identity);
        void OnSignOut();
        string GetSignOutRedirectUri();
        object OnMaintenanceRequest();
        object GetIdentity(System.Security.Claims.ClaimsPrincipal user);
        string GetIdentifier(System.Security.Claims.ClaimsPrincipal user);
        T GetIdentity<T>(System.Security.Claims.ClaimsPrincipal user);
        bool OnboardIdentity(Model.Identity model);
    }
}
