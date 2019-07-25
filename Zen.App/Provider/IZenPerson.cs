using System.Collections.Generic;
using Zen.Base.Module.Data.CommonAttributes;

namespace Zen.App.Provider
{
    public interface IZenPerson : IDataId, IDataLocator, IDataActive
    {
        List<string> Permissions { get; set; }
        List<IZenGroup> Groups();
        bool HasAnyPermissions(string expression);
        bool HasAnyPermissions(IEnumerable<string> terms);
    }
}