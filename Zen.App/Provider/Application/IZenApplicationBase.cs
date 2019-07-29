using Zen.Base.Module.Data.CommonAttributes;

namespace Zen.App.Provider
{
    public interface IZenApplicationBase : IDataId, IDataLocator, IDataCode, IDataActive
    {
        string Name { get; set; }
    }
}