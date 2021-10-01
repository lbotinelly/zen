using System.Security.Principal;
using Zen.Base.Common;

namespace Zen.Web.Auth.Handlers
{
    public interface IAuthEventHandler : IZenProvider
    {
        void OnConfirmSignIn(IIdentity identity);
        void OnSignOut();
        string GetSignOutRedirectUri();
        object OnMaintenanceRequest();
        object GetIdentity();
        bool OnboardIdentity(Model.Identity model);
    }
}
