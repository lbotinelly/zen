using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Internal;

namespace Zen.Web.Discovery.Controller
{
    [Route("api/discovery/[controller]"), ApiController]
    public class RouteController : Microsoft.AspNetCore.Mvc.Controller
    {
        private readonly IActionDescriptorCollectionProvider _actionDescriptorCollectionProvider;

        public RouteController(IActionDescriptorCollectionProvider actionDescriptorCollectionProvider)
        {
            _actionDescriptorCollectionProvider = actionDescriptorCollectionProvider;
        }

        public class EndpointDescriptor
        {
            public string Url;
            public List<string> Methods = new List<string>();
            public string Controller;
        }


        [HttpGet("")]
        public List<EndpointDescriptor> GetAllRoutes()
        {
            // get all available routes
            var routes = _actionDescriptorCollectionProvider.ActionDescriptors.Items.Where(ad => ad.AttributeRouteInfo != null).OrderBy(x => x.AttributeRouteInfo.Template).ToList();

            var cache = new Dictionary<string, EndpointDescriptor>();

            // build response content
            foreach (var route in routes)
            {
                try
                {
                    route.RouteValues.TryGetValue("action", out var action);
                    route.RouteValues.TryGetValue("controller", out var controller);


                    if (!cache.ContainsKey(route.AttributeRouteInfo.Template))
                        cache[route.AttributeRouteInfo.Template] = new EndpointDescriptor()
                        {
                            Url = route.AttributeRouteInfo.Template,
                            Controller =  controller
                        };

                    if (route.ActionConstraints == null) continue;

                    foreach (var actionConstraintMetadata in route.ActionConstraints)
                        if (actionConstraintMetadata is HttpMethodActionConstraint acm)
                            cache[route.AttributeRouteInfo.Template].Methods = acm.HttpMethods.ToList();





                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }

            return cache.Values.ToList();


        }
    }
}