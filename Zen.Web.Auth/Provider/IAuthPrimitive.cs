using Zen.Base.Common;

namespace Zen.Web.Auth.Provider
{
    public interface IAuthPrimitive: IZenProvider
    {
        string Code { get; }
    }
}