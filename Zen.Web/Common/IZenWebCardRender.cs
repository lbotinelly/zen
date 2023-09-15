using Microsoft.AspNetCore.Http;

namespace Zen.Web.Common
{
    public interface IZenWebCardRender
    {
        ZenWebCardDetails GetCardDetails(HttpRequest request);
    }
}
