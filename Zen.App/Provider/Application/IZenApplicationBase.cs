using Zen.Base.Module.Data.CommonAttributes;

namespace Zen.App.Provider.Application
{
    public interface IZenApplicationBase : IDataId, IDataLocator, IDataCode, IDataActive
    {
        string Name { get; set; }
    }
}