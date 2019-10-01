using System.Collections.Generic;

namespace Zen.App.Provider.Application
{
    public interface IZenApplication : IZenApplicationBase
    {
        List<IZenPermission> GetPermissions();
        IZenGroup GetGroup(string code);
    }
}