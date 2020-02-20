using System.Collections.Generic;
using Zen.Base.Module.Data.CommonAttributes;

namespace Zen.App.Core.Group {
    public interface IGroupBase : IDataId, IDataCode, IDataActive
    {
        List<string> Permissions { get; set; }
        string Name { get; set; }
        bool? FromSettings { get; set; }
        string ParentId { get; set; }
        string ApplicationId { get; set; }
    }
}