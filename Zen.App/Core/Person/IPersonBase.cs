using Zen.Base.Module.Data.CommonAttributes;

namespace Zen.App.Core.Person {
    public interface IPersonBase : IDataId, IDataLocator, IDataActive
    {
        string Email { get; set; }
        string Name { get; set; }

    }
}