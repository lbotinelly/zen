using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Zen.Base.Extension;
using Zen.Base.Module;
using Zen.Base.Module.Data;
using Zen.Base.Module.Data.CommonAttributes;
using Zen.Base.Module.Log;
using Zen.Web.Data.Controller.Attributes;
using Zen.Web.Filter;
using static Zen.Base.Module.Data.QueryTransform;

// ReSharper disable InconsistentlySynchronizedField
// ReSharper disable StaticMemberInGenericType
// ReSharper disable InconsistentNaming

namespace Zen.Web.Data.Controller
{
    [ApiController]
    public abstract class DataController<T> : ControllerBase where T : Data<T>
    {
        private static readonly ConcurrentDictionary<Type, EndpointConfiguration> _attributeResolutionCache = new ConcurrentDictionary<Type, EndpointConfiguration>();
        private static readonly object _lockObject = new object();
        private Mutator _mutator;

        public Mutator RequestMutator
        {
            get
            {
                if (_mutator != null) return _mutator;

                var mutator = Request.Query.ToMutator<T>();

                if (Configuration?.Behavior?.MustPaginate == true)
                    if (mutator.Transform.Pagination == null)
                        mutator.Transform.Pagination = new Pagination();

                _mutator = mutator;

                return _mutator;
            }
            set => _mutator = value;
        }

        #region helpers

        [NonAction]
        internal IActionResult PrepareResponse(object content = null, EActionScope scope = EActionScope.Collection, T model = null, HttpStatusCode status = HttpStatusCode.OK)
        {
            var accessControl = new DataAccessControl
            {
                Read = CanRead,
                Write = CanWrite,
                Remove = CanRemove
            };

            Response.Headers?
                .AddModelHeaders(ref accessControl, Request.Query, scope, model)?
                .AddHeaders(GetAccessHeaders(accessControl))?
                .AddMutatorHeaders<T>(RequestMutator);

            var result = new ObjectResult(content) { StatusCode = (int)status };
            return result;
        }

        [NonAction]
        public virtual Dictionary<string, object> GetAccessHeaders(DataAccessControl accessControl) => accessControl.GetAccessHeaders();

        #endregion

        #region Configuration, Authentication and Authorization

        public virtual EndpointConfiguration Configuration
        {
            get
            {
                var currentType = GetType();

                if (_attributeResolutionCache.ContainsKey(currentType)) return _attributeResolutionCache[currentType];

                lock (_lockObject)
                {
                    if (_attributeResolutionCache.ContainsKey(currentType)) return _attributeResolutionCache[currentType];

                    var newDefinition = new EndpointConfiguration
                    {
                        Security = (DataSecurityAttribute)Attribute.GetCustomAttribute(currentType, typeof(DataSecurityAttribute)),
                        Behavior = (DataBehaviorAttribute)Attribute.GetCustomAttribute(currentType, typeof(DataBehaviorAttribute))
                    };

                    _attributeResolutionCache.TryAdd(currentType, newDefinition);

                    return newDefinition;
                }
            }
        }

        [NonAction]
        public void EvaluateAuthorization(EHttpMethod method, EActionType accessType, EActionScope scope, string key = null, T model = null, string context = null)
        {
            var configuration = Configuration;

            if (key == null) key = model != null ? Data<T>.GetDataKey(model) : null;

            if (configuration == null) return;

            string targetPermissionSet;

            switch (accessType)
            {
                case EActionType.Read:
                    targetPermissionSet = configuration.Security?.ReadPermission;
                    break;
                case EActionType.Insert:
                case EActionType.Update:
                    targetPermissionSet = configuration.Security?.WritePermission;
                    break;
                case EActionType.Remove:
                    targetPermissionSet = configuration.Security?.RemovePermission;
                    break;
                default: throw new ArgumentOutOfRangeException(nameof(accessType), accessType, null);
            }

            if (!string.IsNullOrEmpty(targetPermissionSet))
                if (!CheckPermissions(targetPermissionSet))
                    throw new UnauthorizedAccessException("Not authorized.");

            try
            {
                if (AuthorizeAction(method, accessType, scope, key, ref model, context)) return;
            }
            catch (Exception e) // User may throw a custom error, and that's fine: let's log it and re-thrown a custom exception.
            {
                Base.Current.Log.Warn<T>($"AUTH DENIED {method} ({accessType}) [{key}]. Reason: {e.Message}");
                throw new UnauthorizedAccessException("Not authorized: " + e.Message);
            }

            Base.Current.Log.Warn<T>($"Auth DENIED {method} ({accessType}) [{key ?? "(no key)"}]");
            throw new UnauthorizedAccessException("Not authorized.");
        }

        protected bool CanRead => Configuration.Security == null || CheckPermissions(Configuration.Security.ReadPermission);
        protected bool CanWrite => Configuration.Security == null || CheckPermissions(Configuration.Security.WritePermission);
        protected bool CanRemove => Configuration.Security == null || CheckPermissions(Configuration.Security.RemovePermission);

        [NonAction]
        public virtual bool CheckPermissions(string permission) => true;

        #endregion

        #region Event hooks and Behavior modifiers

        [NonAction]
        public virtual bool AuthorizeAction(EHttpMethod method, EActionType pAccessType, EActionScope scope, string key, ref T model, string context) => true;

        [NonAction]
        public virtual void BeforeCollectionAction(EHttpMethod method, EActionType type, ref Mutator mutator, ref IEnumerable<T> model, string key = null) { }

        [NonAction]
        public virtual void AfterCollectionAction(EHttpMethod method, EActionType type, Mutator mutator, ref IEnumerable<T> model, string key = null) { }

        [NonAction]
        public virtual void BeforeModelAction(EHttpMethod method, EActionType type, ref Mutator mutator, ref T model, T originalModel = null, string key = null) { }

        [NonAction]
        public virtual void AfterModelAction(EHttpMethod method, EActionType type, Mutator mutator, ref T model, T originalModel = null, string key = null) { }

        [NonAction]
        public virtual object BeforeModelEmit(EHttpMethod method, EActionType type, Mutator mutator, T model, T originalModel = null, string key = null) => null;

        [NonAction]
        public virtual object BeforeCollectionEmit(EHttpMethod method, EActionType type, Mutator mutator, List<JObject> set) => null;

        [NonAction]
        public virtual void BeforeModelFetch(EHttpMethod method, EActionType type, ref Mutator mutator, ref string key) { }

        #endregion

        #region HTTP Methods

        [NonAction]
        public virtual IEnumerable<T> FetchCollection()
        {
            EvaluateAuthorization(EHttpMethod.Get, EActionType.Read, EActionScope.Collection);
            var mutator = RequestMutator;
            IEnumerable<T> collection = new List<T>();

            BeforeCollectionAction(EHttpMethod.Get, EActionType.Read, ref mutator, ref collection);

            collection = Data<T>.Query(mutator);

            AfterCollectionAction(EHttpMethod.Get, EActionType.Read, mutator, ref collection);

            return collection;
        }

        [NonAction]
        private static object TransformResult(IEnumerable<T> collection, EOutputMapping transform)
        {
            object ret = collection;

            switch (transform)
            {
                case EOutputMapping.Hashmap:
                    ret = collection.ToReference().ToDictionary(i => i.Key, i => i.Display);
                    break;

                case EOutputMapping.Map:
                    ret = collection.ToReference();
                    break;
            }

            return ret;
        }

        [HttpGet("", Order = 999)]
        public IActionResult GetCollection()
        {
            var tl = new TimeLog().Start();

            try
            {
                var collection = FetchCollection();
                object outputCollection = collection;


                if (RequestMutator.Transform != null)
                    if (RequestMutator.Transform.OutputMapping != EOutputMapping.NotSpecified)
                        outputCollection = TransformResult(collection, RequestMutator.Transform.OutputMapping);
                    else
                        if (Interceptors.DataControllerPostFetchInterceptors.Count > 0)
                    {
                        var mutator = Request.Query.ToMutator<T>();

                        var serializer = JsonSerializer.CreateDefault(new JsonSerializerSettings { Converters = { new StringEnumConverter() } });

                        List<JObject> bufferCollection = collection.Select(i => (JObject)JToken.FromObject(i, serializer) ).ToList();

                        foreach (var interceptor in Interceptors.DataControllerPostFetchInterceptors)
                            bufferCollection = interceptor.HandleCollection(bufferCollection, Request) ?? bufferCollection;

                        var payload = BeforeCollectionEmit(EHttpMethod.Get, EActionType.Read, mutator, bufferCollection) ?? bufferCollection;

                        outputCollection = payload;
                    }

                return PrepareResponse(outputCollection);
            }
            catch (Exception e)
            {
                Base.Current.Log.Warn<T>($"GET: {e.Message}");
                Base.Current.Log.Add<T>(e);
                throw;
            }
            finally
            {
                tl.End();
            }
        }

        [HttpGet("new", Order = 999)]
        [AllProperties]
        public virtual IActionResult GetNewModel()
        {
            var tl = new TimeLog().Start();

            try
            {
                tl.Log("EvaluateAuthorization");
                EvaluateAuthorization(EHttpMethod.Get, EActionType.Read, EActionScope.Model);

                T model = null;
                var mutator = RequestMutator;

                BeforeModelAction(EHttpMethod.Get, EActionType.Read, ref mutator, ref model, null, "new");

                tl.Log("Fetch");
                model = typeof(T).CreateInstance<T>();

                AfterModelAction(EHttpMethod.Get, EActionType.Read, mutator, ref model, null, "new");

                tl.Log("Return content");
                return PrepareResponse(model, EActionScope.Model, model);
            }
            catch (Exception e)
            {
                Base.Current.Log.Warn<T>($"GET NEW: {e.Message}");
                Base.Current.Log.Add<T>(e);

                tl.Log(e.FancyString());
                throw;
            }
            finally
            {
                tl.End();
            }
        }

        [NonAction]
        public virtual T FetchModel(string key, ref Mutator mutator)
        {
            EvaluateAuthorization(EHttpMethod.Get, EActionType.Read, EActionScope.Model, key);
            T model = null;

            BeforeModelAction(EHttpMethod.Get, EActionType.Read, ref mutator, ref model, null, key);

            model = GetByLocatorOrKey(key, mutator);

            if (model == null) return null;

            AfterModelAction(EHttpMethod.Get, EActionType.Read, mutator, ref model, null, key);

            return model;
        }

        [HttpGet("{key}", Order = 999)]
        [AllProperties]
        public virtual IActionResult GetModel(string key)
        {
            try
            {
                var mutator = RequestMutator;

                var model = FetchModel(key, ref mutator);

                var payload = BeforeModelEmit(EHttpMethod.Get, EActionType.Read, mutator, model) ?? model;

                return PrepareResponse(payload, EActionScope.Model, model);
            }
            catch (Exception e)
            {
                Base.Current.Log.Warn<T>($"GET {key}: {e.Message}");
                Base.Current.Log.Add<T>(e);
                throw;
            }
        }

        [HttpPost("", Order = 999)]
        public virtual ActionResult<T> PostModel([FromBody] T model)
        {
            try
            {
                if (model == null)
                {

                    Base.Current.Log.Warn<T>($"POST : Invalid model (NULL)");
                    return new BadRequestResult();

                }

                EvaluateAuthorization(EHttpMethod.Post, EActionType.Update, EActionScope.Model, model.GetDataKey(), model);
                var mutator = RequestMutator;

                BeforeModelAction(EHttpMethod.Post, EActionType.Update, ref mutator, ref model);

                model = model.Save(mutator);

                AfterModelAction(EHttpMethod.Post, EActionType.Update, mutator, ref model);

                return new ActionResult<T>(model);
            }
            catch (Exception e)
            {
                Base.Current.Log.Warn<T>($"POST {model.ToJson()}: {e.Message}");
                Base.Current.Log.Add<T>(e);
                throw;
            }
        }

        [HttpDelete(Order = 998)]
        public virtual ActionResult<T> RemoveModelByQuery([FromQuery] string key)
        {
            if (key == null) return new BadRequestResult();

            return RemoveModel(key);
        }

        [HttpDelete("{key}", Order = 999)]
        public virtual ActionResult<T> RemoveModel(string key)
        {
            try
            {

                EvaluateAuthorization(EHttpMethod.Delete, EActionType.Remove, EActionScope.Model, key);

                var mutator = RequestMutator;

                BeforeModelFetch(EHttpMethod.Delete, EActionType.Remove, ref mutator, ref key);

                var model = GetByLocatorOrKey(key, mutator);

                if (model == null) return NotFound();

                BeforeModelAction(EHttpMethod.Delete, EActionType.Read, ref mutator, ref model, model, key);

                model.Remove(mutator);

                AfterModelAction(EHttpMethod.Delete, EActionType.Read, mutator, ref model, model, key);

                return new ActionResult<T>(model);
            }
            catch (Exception e)
            {
                Base.Current.Log.Warn<T>($"DELETE {key}: {e.Message}");
                Base.Current.Log.Add<T>(e);
                throw;
            }
        }

        [NonAction]
        public T GetByLocatorOrKey(string referenceCode, Mutator mutator) => typeof(IDataLocator).IsAssignableFrom(typeof(T)) ? Data<T>.GetByLocator(referenceCode, mutator) ?? Data<T>.Get(referenceCode, mutator) : Data<T>.Get(referenceCode, mutator);

        [HttpPatch("{key}", Order = 999)]
        public virtual ActionResult<T> PatchModel(string key, [FromBody] JsonPatchDocument<T> patchPayload)
        {
            try
            {
                EvaluateAuthorization(EHttpMethod.Patch, EActionType.Update, EActionScope.Model, key);

                var mutator = RequestMutator;

                BeforeModelFetch(EHttpMethod.Patch, EActionType.Update, ref mutator, ref key);

                var originalModel = GetByLocatorOrKey(key, mutator);
                if (originalModel == null) return NotFound();

                var patchedModel = GetByLocatorOrKey(key, mutator);

                patchPayload.ApplyTo(patchedModel);

                // Mismatched keys, so no go.
                if (patchedModel.GetDataKey() != originalModel.GetDataKey()) return new ConflictResult();

                BeforeModelAction(EHttpMethod.Patch, EActionType.Update, ref mutator, ref patchedModel, originalModel, key);

                patchedModel.Save(mutator);

                AfterModelAction(EHttpMethod.Delete, EActionType.Read, mutator, ref patchedModel, originalModel, key);

                return new ActionResult<T>(patchedModel);
            }
            catch (Exception e)
            {
                Base.Current.Log.Warn<T>($"PATCH {key}: {e.Message}");
                Base.Current.Log.Add<T>(e);
                throw;
            }
        }

        #endregion
    }
}