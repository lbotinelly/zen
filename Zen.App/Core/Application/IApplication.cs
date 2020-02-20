using System.Collections.Generic;
using Zen.App.Core.Group;
using Zen.App.Provider;

namespace Zen.App.Core.Application
{
    public interface IApplication : IApplicationBase
    {
        List<IPermission> GetPermissions();
        List<IGroup> GetGroups();
        IGroup GetGroup(string code);
    }
}