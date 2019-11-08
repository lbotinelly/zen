﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Net;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Zen.Base.Extension;
using Zen.Base.Module;
using Zen.Base.Module.Data;
using Zen.Base.Module.Data.CommonAttributes;
using Zen.Web.Data.Controller.Attributes;
using Zen.Web.Filter;

// ReSharper disable InconsistentlySynchronizedField
// ReSharper disable StaticMemberInGenericType
// ReSharper disable InconsistentNaming

namespace Zen.Web.Data.Controller
{
    [Route("api/[controller]"), ApiController]
    public class DataController<T> : ControllerBase where T : Data<T>
    {
        private static readonly ConcurrentDictionary<Type, EndpointConfiguration> _attributeResolutionCache = new ConcurrentDictionary<Type, EndpointConfiguration>();

        private static readonly object _lockObject = new object();
        private Mutator _mutator;

        internal Mutator RequestMutator
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
            var accessControl = new DataAccessControl
            {
                Read = CanRead,
                Write = CanWrite,
                Remove = CanRemove
            };

            Response.Headers.AddModelHeaders<T>(ref accessControl, Request.Query);
            Response.Headers.AddHeaders(GetAccessHeaders(accessControl));
            Response.Headers.AddMutatorHeaders<T>(RequestMutator);

            var result = new ObjectResult(content) {StatusCode = (int) status};
            return result;
        }

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
                        Security = (DataSecurity) Attribute.GetCustomAttribute(currentType, typeof(DataSecurity)),
                        Behavior = (DataBehavior) Attribute.GetCustomAttribute(currentType, typeof(DataBehavior))
                    };

                    _attributeResolutionCache.TryAdd(currentType, newDefinition);

                    return newDefinition;
                }
            }
        }

        internal void EvaluateAuthorization(EHttpMethod method, EActionType accessType, EActionScope scope, string key = null, T model = null, string context = null)
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
                if (!App.Current.Orchestrator?.Person?.HasAnyPermissions(targetPermissionSet) == true)
                    throw new UnauthorizedAccessException("Not authorized.");

            try
            {
                if (AuthorizeAction(method, accessType, scope, key, ref model, context)) return;
            } catch (Exception e) // User may throw a custom error, and that's fine: let's log it and re-thrown a custom exception.
            {
                Base.Current.Log.Warn<T>($"AUTH DENIED {method} ({accessType}) [{key}]. Reason: {e.Message}");
                throw new UnauthorizedAccessException("Not authorized: " + e.Message);
            }

            Base.Current.Log.Warn<T>($"Auth DENIED {method} ({accessType}) [{key ?? "(no key)"}]");
            throw new UnauthorizedAccessException("Not authorized.");
        }

        [NonEvent]
        public virtual Dictionary<string, object> GetAccessHeaders(DataAccessControl accessControl)
        {
            var payload = new List<string>();

            if (accessControl.Read) payload.Add("read");
            if (accessControl.Write) payload.Add("write");
            if (accessControl.Remove) payload.Add("remove");

            return new Dictionary<string, object> { { "x-zen-allowed", payload } };
        }

        protected bool CanRead => Configuration.Security == null || App.Current.Orchestrator.Person?.HasAnyPermissions(Configuration.Security.ReadPermission) == true;
        protected bool CanWrite => Configuration.Security == null || App.Current.Orchestrator.Person?.HasAnyPermissions(Configuration.Security.WritePermission) == true;
        protected bool CanRemove => Configuration.Security == null || App.Current.Orchestrator.Person?.HasAnyPermissions(Configuration.Security.RemovePermission) == true;

        #endregion

        #region Event hooks and Behavior modifiers

        [NonAction]
        public virtual bool AuthorizeAction(EHttpMethod method, EActionType pAccessType, EActionScope scope, string key, ref T model, string context) { return true; }

        [NonAction]
        public virtual void BeforeCollectionAction(EHttpMethod method, EActionType type, ref Mutator mutator, ref IEnumerable<T> model, string key = null) { }

        [NonAction]
        public virtual void AfterCollectionAction(EHttpMethod method, EActionType type, Mutator mutator, ref IEnumerable<T> model, string key = null) { }

        [NonAction]
        public virtual void BeforeModelAction(EHttpMethod method, EActionType type, ref Mutator mutator, ref T model, T originalModel = null, string key = null) { }

        [NonAction]
        public virtual void AfterModelAction(EHttpMethod method, EActionType type, Mutator mutator, ref T model, T originalModel = null, string key = null) { }

        [NonAction]
        public virtual object BeforeModelEmit(EHttpMethod method, EActionType type, Mutator mutator, T model, T originalModel = null, string key = null) { return null; }

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

        private static object TransformResult(IEnumerable<T> collection, string transform)
        {
            object ret = collection;

            switch (transform)
            {
                case "hashmap":
                    ret = collection.ToReference().ToDictionary(i => i.Key, i => i.Display);
                    break;

                case "map":
                    ret = collection.ToReference();
                    break;

                default: break;
            }

            return ret;
        }

        [HttpGet("", Order = 999)]
        public IActionResult GetCollection()
        {
            try
            {
                var collection = FetchCollection();

                var outputCollection = RequestMutator.Transform != null ? TransformResult(collection, RequestMutator.Transform.OutputFormat) : collection;

                return PrepareResponse(outputCollection);
            } catch (Exception e)
            {
                Base.Current.Log.Warn<T>($"GET: {e.Message}");
                Base.Current.Log.Add<T>(e);
                throw;
            }
        }

        [HttpGet("new", Order = 999), AllProperties]
        public virtual ActionResult<T> GetNewModel()
        {
            try
            {
                EvaluateAuthorization(EHttpMethod.Get, EActionType.Read, EActionScope.Model);

                T model = null;
                var mutator = RequestMutator;

                BeforeModelAction(EHttpMethod.Get, EActionType.Read, ref mutator, ref model);

                model = typeof(T).CreateInstance<T>();

                AfterModelAction(EHttpMethod.Get, EActionType.Read, mutator, ref model);
                return new ActionResult<T>(model);
            } catch (Exception e)
            {
                Base.Current.Log.Warn<T>($"GET NEW: {e.Message}");
                Base.Current.Log.Add<T>(e);
                throw;
            }
        }

        [HttpGet("{key}", Order = 999)]
        public virtual ActionResult<object> GetModel(string key)
        {
            try
            {
                EvaluateAuthorization(EHttpMethod.Get, EActionType.Read, EActionScope.Model, key);
                T model = null;
                var mutator = RequestMutator;

                BeforeModelAction(EHttpMethod.Get, EActionType.Read, ref mutator, ref model, null, key);

                model = GetByLocatorOrKey(key, mutator);

                if (model == null) return NotFound();

                AfterModelAction(EHttpMethod.Get, EActionType.Read, mutator, ref model, null, key);

                var payload = BeforeModelEmit(EHttpMethod.Get, EActionType.Read, mutator, model) ?? model;

                return new ActionResult<object>(payload);
            } catch (Exception e)
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
                EvaluateAuthorization(EHttpMethod.Post, EActionType.Update, EActionScope.Model, model.GetDataKey(), model);
                var mutator = RequestMutator;

                BeforeModelAction(EHttpMethod.Post, EActionType.Update, ref mutator, ref model);

                model = model.Save(mutator);

                AfterModelAction(EHttpMethod.Post, EActionType.Update, mutator, ref model);

                return new ActionResult<T>(model);
            } catch (Exception e)
            {
                Base.Current.Log.Warn<T>($"POST {model.ToJson()}: {e.Message}");
                Base.Current.Log.Add<T>(e);
                throw;
            }
        }

        [HttpDelete("{key}", Order = 999)]
        public virtual ActionResult<T> RemoveModel(string key)
        {
            try
            {
                EvaluateAuthorization(EHttpMethod.Delete, EActionType.Remove, EActionScope.Model, key);

                var mutator = RequestMutator;

                var model = GetByLocatorOrKey(key, mutator);

                if (model == null) return NotFound();

                BeforeModelAction(EHttpMethod.Delete, EActionType.Read, ref mutator, ref model, model, key);

                model.Remove(mutator);

                AfterModelAction(EHttpMethod.Delete, EActionType.Read, mutator, ref model, model, key);

                return new ActionResult<T>(model);
            } catch (Exception e)
            {
                Base.Current.Log.Warn<T>($"DELETE {key}: {e.Message}");
                Base.Current.Log.Add<T>(e);
                throw;
            }
        }

        private static T GetByLocatorOrKey(string referenceCode, Mutator mutator) { return typeof(IDataLocator).IsAssignableFrom(typeof(T)) ? Data<T>.GetByLocator(referenceCode, mutator) ?? Data<T>.Get(referenceCode, mutator) : Data<T>.Get(referenceCode, mutator); }

        [HttpPatch("{key}", Order = 999)]
        public virtual ActionResult<T> PatchModel(string key, [FromBody] JsonPatchDocument<T> patchPayload)
        {
            try
            {
                EvaluateAuthorization(EHttpMethod.Patch, EActionType.Update, EActionScope.Model, key);

                var mutator = RequestMutator;

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
            } catch (Exception e)
            {
                Base.Current.Log.Warn<T>($"PATCH {key}: {e.Message}");
                Base.Current.Log.Add<T>(e);
                throw;
            }
        }

        #endregion
    }
}