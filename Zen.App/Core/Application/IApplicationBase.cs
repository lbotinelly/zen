using Zen.Base.Module.Data.CommonAttributes;

namespace Zen.App.Core.Application
{
    public interface IApplicationBase : IDataId, IDataLocator, IDataCode, IDataActive
    {
        string Name { get; set; }
    }
}