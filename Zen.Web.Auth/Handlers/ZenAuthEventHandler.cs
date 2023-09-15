using Zen.Base.Common;
using System.Security.Principal;
using System.Security.Claims;
using System.Linq;

namespace Zen.Web.Auth.Handlers
{
    [Priority(Level = -99)]
    public class ZenAuthEventHandler : 
        IAuthEventHandler
    {
        public bool OnboardIdentity(Model.Identity model)
        {
            return true;
        }

        public EOperationalStatus OperationalStatus => EOperationalStatus.Initialized;
        public object GetIdentity() => null;
        public string GetSignOutRedirectUri() => null;
        public string GetState() => null;
        public void Initialize() { }
        public void OnConfirmSignIn(IIdentity identity) {
        }
        public object OnMaintenanceRequest() => null;
        public void OnSignOut() { }

        public object GetIdentity(ClaimsPrincipal user)
        {
            var identityClaim = ((ClaimsIdentity)user.Identity).Claims;
            return user;
        }

        public T GetIdentity<T>(ClaimsPrincipal user)
        {
            throw new System.NotImplementedException();
        }

        public string GetIdentifier(ClaimsPrincipal user)
        {
            throw new System.NotImplementedException();
        }
    }
}
