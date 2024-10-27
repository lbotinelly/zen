using System.Buffers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Zen.Web.Filter
{
    public class AllPropertiesAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuted(ActionExecutedContext ctx)
        {
            if (!(ctx.Result is ObjectResult objectResult)) return;

            var serializer = new JsonSerializerSettings { NullValueHandling = NullValueHandling.Include };
            serializer.Converters.Add(new StringEnumConverter());

            var formatter = new NewtonsoftJsonOutputFormatter(serializer, ctx.HttpContext.RequestServices.GetRequiredService<ArrayPool<char>>(), new MvcOptions());

            objectResult.Formatters.Add(formatter);
        }
    }
}