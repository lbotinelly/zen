using System.Collections.Generic;
using System.Security.Principal;
using Zen.Base.Common;
using Zen.Base.Module;

namespace Zen.Web.Auth.Handlers
{
    public interface IAuthEventHandler : IZenProvider
    {
        void OnConfirmSignIn(IIdentity identity);
        void OnSignOut();
        string GetSignOutRedirectUri();
        object OnMaintenanceRequest();
        object GetIdentity(System.Security.Claims.ClaimsPrincipal user);
        T GetIdentity<T>(System.Security.Claims.ClaimsPrincipal user);
        bool OnboardIdentity(Model.Identity model);
    }
}
