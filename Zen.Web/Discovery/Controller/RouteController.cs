using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Zen.Base.Extension;

namespace Zen.Web.Discovery.Controller
{
    [Route("api/discovery/endpoint")]
    public class RouteController : ControllerBase
    {
        private readonly IActionDescriptorCollectionProvider _actionDescriptorCollectionProvider;

        public RouteController(IActionDescriptorCollectionProvider actionDescriptorCollectionProvider) { _actionDescriptorCollectionProvider = actionDescriptorCollectionProvider; }

        [HttpGet("")]
        public List<EndpointDescriptor> GetAllRoutes()
        {
            // get all available routes
            var routes = _actionDescriptorCollectionProvider
                .ActionDescriptors
                .Items
                .Where(ad => ad.AttributeRouteInfo!= null)
                .OrderBy(x => x.AttributeRouteInfo.Template)
                .ToList();

            var cache = new Dictionary<string, EndpointDescriptor>();

            // build response content
            foreach (var route in routes)
                try
                {
                    if (!cache.ContainsKey(route.AttributeRouteInfo.Template))
                        cache[route.AttributeRouteInfo.Template] = new EndpointDescriptor
                        {
                            Url = route.AttributeRouteInfo.Template
                        };

                    cache[route.AttributeRouteInfo.Template].Parameters = route.Parameters?.Select(p => new
                    {
                        Payload =
                            !p.ParameterType.IsBasicType() ? p.ParameterType.GetConstructor(new Type[] { }).Invoke(new object[] { }) : "",
                        p.Name
                    }).ToList();

                    if (route.ActionConstraints == null) continue;

                    foreach (var actionConstraintMetadata in route.ActionConstraints)
                        if (actionConstraintMetadata is HttpMethodActionConstraint acm)
                            cache[route.AttributeRouteInfo.Template].Methods = acm.HttpMethods.ToList();
                } catch (Exception e) { Console.WriteLine(e); }

            return cache.Values.ToList();
        }

        public class EndpointDescriptor
        {
            public List<string> Methods = new List<string>();
            public string Url;
            public object Parameters { get; set; }
        }
    }
}