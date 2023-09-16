using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Zen.Base.Module;
using Zen.Base.Module.Data;
using Zen.Web.Filter;

namespace Zen.Web.Data.Controller
{
    [ApiController]
    public abstract class DataController<T, TSummary> : DataController<T> where T : Data<T>
    {
        [NonAction]
        public virtual void BeforeSummaryCollectionAction(EHttpMethod method, EActionType type, ref Mutator mutator, ref IEnumerable<TSummary> model, string key = null) { }
        [NonAction]
        public virtual void AfterSummaryCollectionAction(EHttpMethod method, EActionType type, Mutator mutator, ref IEnumerable<TSummary> model, string key = null) { }

        [HttpGet("summary", Order = 998)]
        [AllProperties]
        public virtual IActionResult GetSummary()
        {
            try
            {
                EvaluateAuthorization(EHttpMethod.Get, EActionType.Read, EActionScope.Collection);
                var mutator = RequestMutator;
                IEnumerable<TSummary> collection = new List<TSummary>();

                BeforeSummaryCollectionAction(EHttpMethod.Get, EActionType.Read, ref mutator, ref collection);

                collection = Data<T>.Query<TSummary>(mutator);

                AfterSummaryCollectionAction(EHttpMethod.Get, EActionType.Read, mutator, ref collection);

                var response = BeforeSummaryCollectionEmit(EHttpMethod.Get, EActionType.Read, mutator, collection) ?? collection;

                return PrepareResponse(response);
            }
            catch (Exception e)
            {
                Base.Current.Log.Warn<T>($"SUMMARY: {e.Message}");
                Base.Current.Log.Add<T>(e);
                throw;
            }
        }
        [NonAction]
        public virtual object BeforeSummaryCollectionEmit(EHttpMethod method, EActionType type, Mutator mutator, IEnumerable<TSummary> set) => null;

    }
}