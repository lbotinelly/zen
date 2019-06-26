using System.Collections.Generic;
using System.Security.Principal;
using Zen.Base.Common;

namespace Zen.Base.Module.Identity
{
    public interface IAuthorizationProvider : IZenProvider
    {
        IIdentity Identity { get; }
        string Id { get; }
        string Locator { get; }
        bool CheckPermission(string pCode);
        bool CheckPermission(IEnumerable<string> pCode);
    }
}