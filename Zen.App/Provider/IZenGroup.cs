using System.Collections.Generic;
using Zen.Base.Module.Data.CommonAttributes;
using static Zen.App.Orchestrator.Model.Application;

namespace Zen.App.Provider
{
    public interface IZenGroup : IDataId, IDataCode, IDataActive 
    {
        List<Permission> Permissions { get; set; }
        bool FromSettings { get; set; }
        string ParentId { get; set; }
    }
}