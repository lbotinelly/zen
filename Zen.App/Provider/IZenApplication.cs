using System.Collections.Generic;
using Zen.Base.Module.Data.CommonAttributes;

namespace Zen.App.Provider
{
    public interface IZenApplication : IDataId, IDataLocator, IDataCode, IDataActive
    {
        string Name { get; set; }
        List<IZenPermission> GetPermissions();
    }
}