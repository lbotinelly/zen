using Zen.Web.Auth.Handlers;
using Zen.Base.Common;
using Zen.Web.Auth.Model;
using System.Security.Principal;
using System.Security.Claims;

namespace Zen.Provider.OpenId.Authentication
{
    [Priority(Level = -98)]
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

        public object GetIdentity(ClaimsPrincipal user)
        {
            throw new System.NotImplementedException();
        }

        public T GetIdentity<T>(ClaimsPrincipal user)
        {
            throw new System.NotImplementedException();
        }
    }
}