using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Zen.Base;
using Zen.Base.Extension;
using Zen.Base.Module;
using Zen.Base.Module.Data;
using Zen.Base.Module.Identity;

// ReSharper disable InconsistentlySynchronizedField
// ReSharper disable StaticMemberInGenericType
// ReSharper disable InconsistentNaming

namespace Zen.Module.Web.Controller
{
    [Route("api/[controller]"), ApiController]
    public class DataController<T> : Microsoft.AspNetCore.Mvc.Controller where T : Data<T>
    {
        private Mutator QueryStringMutator => Request.Query.ToMutator();

        private static readonly ConcurrentDictionary<Type, EndpointConfiguration> _attributeResolutionCache = new ConcurrentDictionary<Type, EndpointConfiguration>();
        private static readonly object _lockObject = new object();

        #region helpers

        internal IActionResult PrepareResponse(object content = null, Mutator mutator = null, HttpStatusCode status = HttpStatusCode.OK)
        {
            Response.Headers.AddModelHeaders<T>();
            Response.Headers.AddHeaders(GetAccessHeadersByPermission());

            //if (mutator != null) Response.Headers.AddHeaders(mutator.ToHeaders());

            var result = new ObjectResult(content) {StatusCode = (int) status};
            return result;
        }


        #endregion

        #region Configuration, Authentication and Authorization

        public EndpointConfiguration Configuration
        {
            get
            {
                var typeRef = GetType();

                if (_attributeResolutionCache.ContainsKey(typeRef)) return _attributeResolutionCache[typeRef];

                lock (_lockObject)
                {
                    if (_attributeResolutionCache.ContainsKey(typeRef)) return _attributeResolutionCache[typeRef];

                    var newDefinition = new EndpointConfiguration
                    {
                        Security = (EndpointConfiguration.SecurityAttribute)
                            Attribute.GetCustomAttribute(typeRef, typeof(EndpointConfiguration.SecurityAttribute)),
                        Behavior = (EndpointConfiguration.BehaviorAttribute)
                            Attribute.GetCustomAttribute(typeRef, typeof(EndpointConfiguration.BehaviorAttribute))
                    };

                    _attributeResolutionCache.TryAdd(typeRef, newDefinition);

                    return newDefinition;
                }
            }
        }

        private void EvaluateAuthorization(EHttpMethod method, EActionType accessType, EActionScope scope, string key = null, T model = null, string context = null)
        {
            var configuration = Configuration;

            if (key == null) key = model != null ? Data<T>.GetDataKey(model) : "";

            if (configuration == null) return;

            string targetPermissionSet;

            switch (accessType)
            {
                case EActionType.Read:
                    targetPermissionSet = configuration.Security.Read;
                    break;
                case EActionType.Insert:
                case EActionType.Update:
                    targetPermissionSet = configuration.Security.Write;
                    break;
                case EActionType.Remove:
                    targetPermissionSet = configuration.Security.Remove;
                    break;
                default: throw new ArgumentOutOfRangeException(nameof(accessType), accessType, null);
            }

            if (!IdentityHelper.HasAnyPermissions(targetPermissionSet)) throw new UnauthorizedAccessException("Not authorized.");

            try
            {
                if (AuthorizeAction(method, accessType, scope, key, ref model, context)) return;
            } catch (Exception e) // User may throw a custom error, and that's fine: let's log it and re-thrown a custom exception.
            {
                Current.Log.Warn<T>($"AUTH DENIED {method} ({accessType}) [{key}]. Reason: {e.Message}");
                throw new UnauthorizedAccessException("Not authorized: " + e.Message);
            }

            Current.Log.Warn<T>($"Auth DENIED {method} ({accessType}) [{key ?? "(no key)"}]");
            throw new UnauthorizedAccessException("Not authorized.");
        }

        private Dictionary<string, object> GetAccessHeadersByPermission()
        {
            var payload = new List<string>();

            if (Configuration.Security == null || IdentityHelper.HasAnyPermissions(Configuration.Security.Read)) payload.Add("read");
            if (Configuration.Security == null || IdentityHelper.HasAnyPermissions(Configuration.Security.Write)) payload.Add("write");
            if (Configuration.Security == null || IdentityHelper.HasAnyPermissions(Configuration.Security.Remove)) payload.Add("remove");

            return new Dictionary<string, object> {{"x-zen-allowed", payload}};
        }

        #endregion

        #region Event hooks and Behavior modifiers

        public virtual bool AuthorizeAction(EHttpMethod method, EActionType pAccessType, EActionScope scope, string key, ref T model, string context) { return true; }

        public virtual void BeforeCollectionAction(EHttpMethod method, EActionType type, ref Mutator mutator, ref IEnumerable<T> model, string key = null) { }
        public virtual void AfterCollectionAction(EHttpMethod method, EActionType type, Mutator mutator, ref IEnumerable<T> model, string key = null) { }
        public virtual void BeforeModelAction(EHttpMethod method, EActionType type, ref Mutator mutator, ref T model, T originalModel = null, string key = null) { }
        public virtual void AfterModelAction(EHttpMethod method, EActionType type, Mutator mutator, ref T model, T originalModel = null, string key = null) { }

        #endregion

        #region HTTP Methods

        [HttpGet("")]
        public IActionResult GetCollection()
        {
            try
            {
                EvaluateAuthorization(EHttpMethod.Get, EActionType.Read, EActionScope.Collection);

                // A GetAll operation is just a query without filters, but sometimes we need something more.
                // So let's give the user the possibility by adding a hook to a modifier pre-processor.
                var mutator = QueryStringMutator;

                IEnumerable<T> collection = new List<T>();

                BeforeCollectionAction(EHttpMethod.Get, EActionType.Read, ref mutator, ref collection);

                collection = Data<T>.Query(mutator);

                AfterCollectionAction(EHttpMethod.Get, EActionType.Read, mutator, ref collection);

                return PrepareResponse(collection);
            } catch (Exception e)
            {
                Current.Log.Warn<T>($"GET: {e.Message}");
                Current.Log.Add<T>(e);
                throw;
            }
        }

        [HttpGet("{key}")]
        public virtual ActionResult<T> GetModel(string key)
        {
            try
            {
                EvaluateAuthorization(EHttpMethod.Get, EActionType.Read, EActionScope.Model, key);

                T model = null;

                var mutator = QueryStringMutator;

                BeforeModelAction(EHttpMethod.Get, EActionType.Read, ref mutator, ref model, null, key);

                model = Data<T>.Get(key, mutator);
                if (model == null) return NotFound();

                AfterModelAction(EHttpMethod.Get, EActionType.Read, mutator, ref model, null, key);
                return new ActionResult<T>(model);
            } catch (Exception e)
            {
                Current.Log.Warn<T>($"GET {key}: {e.Message}");
                Current.Log.Add<T>(e);
                throw;
            }
        }


        [HttpPost("")]
        public virtual ActionResult<T> PostModel([FromBody] T model)
        {
            try
            {
                EvaluateAuthorization(EHttpMethod.Post, EActionType.Update, EActionScope.Model, model.GetDataKey());

                var mutator = QueryStringMutator;

                BeforeModelAction(EHttpMethod.Post, EActionType.Update, ref mutator, ref model);

                model = model.Save(mutator);

                AfterModelAction(EHttpMethod.Post, EActionType.Update, mutator, ref model);

                return new ActionResult<T>(model);
            } catch (Exception e)
            {
                Current.Log.Warn<T>($"POST {model.ToJson()}: {e.Message}");
                Current.Log.Add<T>(e);
                throw;
            }
        }

        [HttpDelete("{key}")]
        public virtual ActionResult<T> RemoveModel(string key)
        {
            try
            {
                EvaluateAuthorization(EHttpMethod.Delete, EActionType.Remove, EActionScope.Model, key);

                var mutator = QueryStringMutator;

                var model = Data<T>.Get(key, mutator);
                if (model == null) return NotFound();

                BeforeModelAction(EHttpMethod.Delete, EActionType.Read, ref mutator, ref model, model, key);

                model.Remove(mutator);

                AfterModelAction(EHttpMethod.Delete, EActionType.Read, mutator, ref model, model, key);

                return new ActionResult<T>(model);

            } catch (Exception e)
            {
                Current.Log.Warn<T>($"DELETE {key}: {e.Message}");
                Current.Log.Add<T>(e);
                throw;
            }
        }

        [HttpPatch("{key}")]
        public virtual ActionResult<T> PatchModel(string key, [FromBody] JsonPatchDocument<T> patchPayload)
        {
            try
            {
                EvaluateAuthorization(EHttpMethod.Patch, EActionType.Update, EActionScope.Model, key);

                var mutator = QueryStringMutator;

                var originalModel = Data<T>.Get(key, mutator);
                if (originalModel == null) return NotFound();

                var patchedModel = Data<T>.Get(key, mutator);

                patchPayload.ApplyTo(patchedModel);

                // Mismatched keys, so no go.
                if (patchedModel.GetDataKey() != originalModel.GetDataKey()) return new ConflictResult();

                BeforeModelAction(EHttpMethod.Patch, EActionType.Update, ref mutator, ref patchedModel, originalModel, key);

                patchedModel.Save(mutator);

                AfterModelAction(EHttpMethod.Delete, EActionType.Read, mutator, ref patchedModel, originalModel, key);

                return new ActionResult<T>(patchedModel);

            } catch (Exception e)
            {
                Current.Log.Warn<T>($"PATCH {key}: {e.Message}");
                Current.Log.Add<T>(e);
                throw;
            }
        }

        #endregion
    }
}