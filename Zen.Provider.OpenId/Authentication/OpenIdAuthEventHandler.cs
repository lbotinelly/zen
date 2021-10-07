using Zen.Web.Auth.Handlers;
using Zen.Base.Common;
using Zen.Web.Auth.Model;
using System.Security.Principal;

namespace Zen.Provider.OpenId.Authentication
{
    public class OpenIdAuthEventHandler : IAuthEventHandler
    {
        public bool OnboardIdentity(Identity model)
        {
            return true;
        }

        public EOperationalStatus OperationalStatus => EOperationalStatus.Initialized;
        public object GetIdentity() => null;
        public string GetSignOutRedirectUri() => null;
        public string GetState() => null;
        public void Initialize() { }
        public void OnConfirmSignIn(IIdentity identity) { }
        public object OnMaintenanceRequest() => null;
        public void OnSignOut() { }
    }
}