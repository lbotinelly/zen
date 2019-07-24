using System.Collections.Generic;
using Zen.Base.Module.Data.CommonAttributes;

namespace Zen.App.Provider
{
    public interface IZenPerson<T> : IDataId, IDataLocator, IDataActive where T : IZenPermission
    {
        bool HasAnyPermissions(string expression);
        bool HasAnyPermissions(IEnumerable<string> terms);
        List<T> Permissions { get; set; }
        List<IZenGroup<T>> Groups();
    }
}