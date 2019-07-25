using System.Collections.Generic;
using Zen.Base.Module.Data.CommonAttributes;

namespace Zen.App.Provider
{
    public interface IZenGroup : IDataId, IDataCode, IDataActive 
    {
        List<string> Permissions { get; set; }
        bool FromSettings { get; set; }
        string ParentId { get; set; }
        string ApplicationId { get; set; }

    }
}