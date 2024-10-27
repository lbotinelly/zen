using Microsoft.AspNetCore.Mvc;
using Zen.Web.Data.Controller;
using Zen.Web.App.Model.Metrics;
using Zen.Base.Module.Data;

namespace Zen.Web.App.Controllers
{
    [Route("metrics/datapoint")]
    public class FavoritesController : DataController<Datapoint>
    {
        public override bool AuthorizeAction(EHttpMethod method, EActionType pAccessType, EActionScope scope, string key, ref Datapoint model, string context)
        {
            if (method != EHttpMethod.Post) return false;

            return base.AuthorizeAction(method, pAccessType, scope, key, ref model, context);
        }

    }
}