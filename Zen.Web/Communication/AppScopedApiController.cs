using System.Collections.Generic;
using System.Net.Http;
using Zen.App.Model.Communication;
using Zen.Base.Module;
using Zen.Base.Module.Data;
using Zen.Web.Data.Controller;

namespace Zen.Web.Communication
{
    public class AppScopedApiController<T> : DataController<T> where T : AppScopedContent<T>
    {
        public string AdminPermission { get; set; }

        public override IEnumerable<T> FetchCollection()
        {
            EvaluateAuthorization(EHttpMethod.Get, EActionType.Read, EActionScope.Collection);
            var mutator = RequestMutator;

            IEnumerable<T> collection = new List<T>();

            BeforeCollectionAction(EHttpMethod.Get, EActionType.Read, ref mutator, ref collection);

            if (App.Current.Orchestrator.Application!= null) mutator.Transform.AddFilter(new { ApplicationCode = App.Current.Orchestrator.Application?.Code });

            if (App.Current.Orchestrator.Person == null || !App.Current.Orchestrator.HasAnyPermissions(AdminPermission))
            {
                mutator.Transform.Filter = Context.GetQueryByContext(new Context.QueryByContextParm { Active = true, WrapOutput = true, StreamType = Context.StreamCutoutType.None });
            }

            collection = Data<T>.Query(mutator);

            AfterCollectionAction(EHttpMethod.Get, EActionType.Read, mutator, ref collection);
            return collection;
        }

        public override T FetchModel(string key, ref Mutator mutator)
        {
            EvaluateAuthorization(EHttpMethod.Get, EActionType.Read, EActionScope.Model, key);
            T model = null;

            BeforeModelAction(EHttpMethod.Get, EActionType.Read, ref mutator, ref model, null, key);

            model = GetByLocatorOrKey(key, mutator);

            if (model == null) return null;

            var currentApplicationCode = App.Current.Orchestrator.Application?.Code;

            if (model.ApplicationCode != currentApplicationCode) throw new HttpRequestException($"Mismatched Application Code [{currentApplicationCode}]");

            if (!model.Audience.IsVisibleToCurrentPerson()) throw new HttpRequestException("User not in intended audience.");

            AfterModelAction(EHttpMethod.Get, EActionType.Read, mutator, ref model, null, key);

            return model;
        }
    }
}