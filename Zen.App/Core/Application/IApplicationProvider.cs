
namespace Zen.App.Core.Application
{
    public interface IApplicationProvider
    {
        IApplication Application { get; }
        IApplication Compile(bool force);
    }
}