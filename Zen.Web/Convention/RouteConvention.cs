using System.Linq;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.Routing;

namespace Zen.Web.Convention
{
    // https://www.strathweb.com/2016/06/global-route-prefix-with-asp-net-core-mvc-revisited/
    public class RouteConvention : IApplicationModelConvention
    {
        private readonly AttributeRouteModel _centralPrefix;

        public RouteConvention(IRouteTemplateProvider routeTemplateProvider) { _centralPrefix = new AttributeRouteModel(routeTemplateProvider); }

        public void Apply(ApplicationModel application)
        {
            foreach (var controller in application.Controllers)
            {
                var matchedSelectors = controller.Selectors.Where(x => x.AttributeRouteModel!= null).ToList();
                if (matchedSelectors.Any())
                    foreach (var selectorModel in matchedSelectors)
                        selectorModel.AttributeRouteModel = AttributeRouteModel.CombineAttributeRouteModel(_centralPrefix, selectorModel.AttributeRouteModel);

                var unmatchedSelectors = controller.Selectors.Where(x => x.AttributeRouteModel == null).ToList();

                if (!unmatchedSelectors.Any()) continue;

                foreach (var selectorModel in unmatchedSelectors)
                    selectorModel.AttributeRouteModel = _centralPrefix;
            }
        }
    }
}