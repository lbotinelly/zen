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
using Zen.Module.Web.Auth;
using Zen.Web.Data.Controller.Attributes;

// ReSharper disable InconsistentlySynchronizedField
// ReSharper disable StaticMemberInGenericType
// ReSharper disable InconsistentNaming

namespace Zen.Web.Data.Controller
{
    [Route("api/[controller]"), ApiController]
    public class DataController<T> : Microsoft.AspNetCore.Mvc.Controller where T : Data<T>
    {
        private static readonly ConcurrentDictionary<Type, EndpointConfiguration> _attributeResolutionCache =
            new ConcurrentDictionary<Type, EndpointConfiguration>();

        private static readonly object _lockObject = new object();
        private Mutator _mutator;

        private Mutator RequestMutator
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

        internal IActionResult PrepareResponse(object content = null, HttpStatusCode status = HttpStatusCode.OK)
        {
            Response.Headers.AddModelHeaders<T>();
            Response.Headers.AddHeaders(GetAccessHeadersByPermission());

            Response.Headers.AddMutatorHeaders<T>(RequestMutator);

            var result = new ObjectResult(content) {StatusCode = (int) status};
            return result;
        }

        #endregion

        #region Configuration, Authentication and Authorization

        public EndpointConfiguration Configuration
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
                        Security = (Security) Attribute.GetCustomAttribute(currentType, typeof(Security)),
                        Behavior = (Behavior) Attribute.GetCustomAttribute(currentType, typeof(Behavior))
                    };

                    _attributeResolutionCache.TryAdd(currentType, newDefinition);

                    return newDefinition;
                }
            }
        }

        private void EvaluateAuthorization(EHttpMethod method, EActionType accessType, EActionScope scope,
                                           string key = null, T model = null, string context = null)
        {
            var configuration = Configuration;

            if (key == null) key = model != null ? Data<T>.GetDataKey(model) : null;

            if (configuration == null) return;

            string targetPermissionSet;

            switch (accessType)
            {
                case EActionType.Read:
                    targetPermissionSet = configuration.Security?.Read;
                    break;
                case EActionType.Insert:
                case EActionType.Update:
                    targetPermissionSet = configuration.Security?.Write;
                    break;
                case EActionType.Remove:
                    targetPermissionSet = configuration.Security?.Remove;
                    break;
                default: throw new ArgumentOutOfRangeException(nameof(accessType), accessType, null);
            }

            if (!IdentityExtensions.HasAnyPermissions(targetPermissionSet)) throw new UnauthorizedAccessException("Not authorized.");

            try
            {
                if (AuthorizeAction(method, accessType, scope, key, ref model, context)) return;
            } catch (Exception e
            ) // User may throw a custom error, and that's fine: let's log it and re-thrown a custom exception.
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

            if (Configuration.Security == null || IdentityExtensions.HasAnyPermissions(Configuration.Security.Read)) payload.Add("read");
            if (Configuration.Security == null || IdentityExtensions.HasAnyPermissions(Configuration.Security.Write)) payload.Add("write");
            if (Configuration.Security == null || IdentityExtensions.HasAnyPermissions(Configuration.Security.Remove)) payload.Add("remove");

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
                var mutator = RequestMutator;
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

        [HttpGet("new")]
        public virtual ActionResult<T> GetNewModel()
        {
            try
            {
                EvaluateAuthorization(EHttpMethod.Get, EActionType.Read, EActionScope.Model, null);

                T model = null;
                var mutator = RequestMutator;

                BeforeModelAction(EHttpMethod.Get, EActionType.Read, ref mutator, ref model, null, null);

                model = typeof(T).CreateInstance<T>();

                AfterModelAction(EHttpMethod.Get, EActionType.Read, mutator, ref model, null, null);
                return new ActionResult<T>(model);
            }
            catch (Exception e)
            {
                Current.Log.Warn<T>($"GET NEW: {e.Message}");
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
                var mutator = RequestMutator;

                BeforeModelAction(EHttpMethod.Get, EActionType.Read, ref mutator, ref model, null, key);

                model = Data<T>.Get(key, mutator);

                if (model == null) return NotFound();

                AfterModelAction(EHttpMethod.Get, EActionType.Read, mutator, ref model, null, key);
                return new ActionResult<T>(model);
            }
            catch (Exception e)
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
                var mutator = RequestMutator;

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

                var mutator = RequestMutator;

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

                var mutator = RequestMutator;

                var originalModel = Data<T>.Get(key, mutator);
                if (originalModel == null) return NotFound();

                var patchedModel = Data<T>.Get(key, mutator);

                patchPayload.ApplyTo(patchedModel);

                // Mismatched keys, so no go.
                if (patchedModel.GetDataKey() != originalModel.GetDataKey()) return new ConflictResult();

                BeforeModelAction(EHttpMethod.Patch, EActionType.Update, ref mutator, ref patchedModel, originalModel,
                                  key);

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