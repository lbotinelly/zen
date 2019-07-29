using Zen.Base.Module.Data.CommonAttributes;

namespace Zen.App.Provider.Person {
    public interface IZenPersonBase : IDataId, IDataLocator, IDataActive
    {
        string Email { get; set; }
        string Name { get; set; }

    }
}