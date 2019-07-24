using System.Collections.Generic;
using Zen.Base.Module.Data.CommonAttributes;

namespace Zen.App.Provider
{
    public interface IZenGroup<T> : IDataId, IDataCode, IDataActive where T: IZenPermission
    {
        List<T> Permissions { get; set; }
        bool FromSettings { get; set; }
        string ParentId { get; set; }
    }
}