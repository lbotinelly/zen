using Zen.Base.Module.Data.CommonAttributes;

namespace Zen.App.Provider
{
    public interface IZenApplication : IDataId, IDataLocator, IDataCode
    {
        string Name { get; set; }
        bool Active { get; set; }
    }
}