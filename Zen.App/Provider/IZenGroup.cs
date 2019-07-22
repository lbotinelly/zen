using Zen.Base.Module.Data.CommonAttributes;

namespace Zen.App.Provider
{
    public interface IZenGroup : IDataId, IDataCode, IDataActive
    {
         bool FromSettings { get; set; }
    }
}