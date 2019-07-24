using System.Collections.Generic;
using Zen.Base.Module.Data.CommonAttributes;
using static Zen.App.Orchestrator.Model.Application;

namespace Zen.App.Provider
{
    public interface IZenPerson : IDataId, IDataLocator, IDataActive
    {
        bool HasAnyPermissions(string expression);
        bool HasAnyPermissions(IEnumerable<string> terms);
        List<Permission> Permissions { get; set; }
        List<IZenGroup> Groups();
    }
}