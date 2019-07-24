using System.Collections.Generic;
using Zen.Base.Module.Data.CommonAttributes;

namespace Zen.App.Provider
{
    public interface IZenApplication<T> : IDataId, IDataLocator, IDataCode, IDataActive where T: IZenPermission
    {
        string Name { get; set; }
        List<T> Permissions { get; set; }
    }
}