using System.Collections.Generic;
using Zen.Base.Module.Data.CommonAttributes;
using static Zen.App.Orchestrator.Model.Application;

namespace Zen.App.Provider
{
    public interface IZenApplication : IDataId, IDataLocator, IDataCode, IDataActive
    {
        string Name { get; set; }
        List<Permission> Permissions { get; set; }
    }
}