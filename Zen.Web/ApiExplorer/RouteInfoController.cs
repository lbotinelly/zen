using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;
using System.Linq;
using System.Text;

namespace Zen.Web.ApiExplorer
{
    [Route("api/diagnostics/routes")]
    public class RouteInfoController : Controller
    {
        // for accessing conventional routes...
        private readonly IActionDescriptorCollectionProvider _actionDescriptorCollectionProvider;

        public RouteInfoController(IActionDescriptorCollectionProvider actionDescriptorCollectionProvider)
        {
            _actionDescriptorCollectionProvider = actionDescriptorCollectionProvider;
        }

        public IActionResult Index()
        {
            StringBuilder sb = new StringBuilder();

            foreach (ActionDescriptor ad in _actionDescriptorCollectionProvider.ActionDescriptors.Items)
            {
                var context = new UrlActionContext()
                {
                    Action = ad.RouteValues["action"],
                    Controller = ad.RouteValues["controller"],
                    Values = ad.RouteValues
                };

                var method = ad.EndpointMetadata.Where(i => i is HttpMethodAttribute).Select(i => (HttpMethodAttribute)i).FirstOrDefault();


                var action = Url.Action(context);

                sb.AppendLine(action).AppendLine().AppendLine();
            }

            return Ok(sb.ToString());
        }

    }
}