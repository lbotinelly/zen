using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Zen.Base;
using Zen.Base.Extension;
using Zen.Base.Module;
using Zen.Base.Module.Data;
using Zen.Base.Module.Data.Adapter;
using Zen.Base.Module.Identity;

// ReSharper disable InconsistentlySynchronizedField
// ReSharper disable StaticMemberInGenericType
// ReSharper disable InconsistentNaming

namespace Zen.Module.Web.Controller
{
    public static class Extentions
    {
        internal static void AddHeaders(this IHeaderDictionary header, Dictionary<string, object> payload)
        {
            if (payload == null) return;

            foreach (var headerItem in payload)
            {
                if (header.ContainsKey(headerItem.Key)) header.Remove(headerItem.Key);
                header.Add(headerItem.Key, headerItem.Value.ToJson());
            }
        }

        internal static void AddModelHeaders<T>(this IHeaderDictionary header) where T : Data<T>
        {
            if (Data<T>.Info<T>.Settings?.Pipelines?.Before != null)
                foreach (var pipelineMember in Data<T>.Info<T>.Settings.Pipelines.Before)
                    AddHeaders(header, pipelineMember.Headers<T>());

            if (Data<T>.Info<T>.Settings?.Pipelines?.After != null)
                foreach (var pipelineMember in Data<T>.Info<T>.Settings.Pipelines.After)
                    AddHeaders(header, pipelineMember.Headers<T>());
        }

        internal static QueryModifier ToQueryModifier(this IQueryCollection source)
        {
            var modifier = new QueryModifier { Payload = new QueryPayload() };

            if (source.ContainsKey("sort")) modifier.Payload.OrderBy = source["sort"];

            if (source.ContainsKey("page") || source.ContainsKey("limit"))
            {
                modifier.Payload.PageIndex = source.ContainsKey("page") ? Convert.ToInt32(source["page"]) : 0;
                modifier.Payload.PageSize = source.ContainsKey("limit") ? Convert.ToInt32(source["limit"]) : 50;
            }

            if (source.ContainsKey("filter")) modifier.Payload.Filter = source["filter"];

            if (source.ContainsKey("q")) modifier.Payload.OmniQuery = source["q"];

            if (source.ContainsKey("set")) modifier.Collection = source["set"];

            return modifier;
        }
    }

    [Route("api/[controller]"), ApiController]
    public class DataController<T> : Microsoft.AspNetCore.Mvc.Controller where T : Data<T>
    {
        // ReSharper disable once StaticMemberInGenericType
        // ReSharper disable once InconsistentNaming
        private static readonly ConcurrentDictionary<Type, EndpointConfiguration> _attributeResolutionCache = new ConcurrentDictionary<Type, EndpointConfiguration>();

        private static readonly object _lockObject = new object();

        #region helpers

        internal IActionResult PrepareResponse(object content = null, QueryModifier modifier = null, HttpStatusCode status = HttpStatusCode.OK)
        {
            Response.Headers.AddModelHeaders<T>();
            Response.Headers.AddHeaders(GetAccessHeadersByPermission());

      //if (modifier != null) Response.Headers.AddHeaders(modifier.ToHeaders());

            var result = new ObjectResult(content) { StatusCode = (int)status };
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

        private void EvaluateAuthorization(ERequestType requestType, EActionType accessType, EActionScope scope, string key = null, T model = null, string context = null)
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
                if (AuthorizeAction(requestType, accessType, key, ref model, context)) return;
            }
            catch (Exception e) // User may throw a custom error, and that's fine: let's log it and re-thrown a custom exception.
            {
                Current.Log.Warn<T>($"AUTH DENIED {requestType} ({accessType}) [{key}]. Reason: {e.Message}");
                throw new UnauthorizedAccessException("Not authorized: " + e.Message);
            }

            Current.Log.Warn<T>("Auth DENIED " + requestType + "(" + accessType + ") [" + (key ?? "(no key)") + "]");
            throw new UnauthorizedAccessException("Not authorized.");
        }

        private Dictionary<string, object> GetAccessHeadersByPermission()
        {
            var payload = new List<string>();

            if (Configuration.Security == null || IdentityHelper.HasAnyPermissions(Configuration.Security.Read)) payload.Add("read");
            if (Configuration.Security == null || IdentityHelper.HasAnyPermissions(Configuration.Security.Write)) payload.Add("write");
            if (Configuration.Security == null || IdentityHelper.HasAnyPermissions(Configuration.Security.Remove)) payload.Add("remove");

            return new Dictionary<string, object> { { "x-zen-allowed", payload } };
        }

        #endregion

        #region Event hooks and Behavior modifiers

        public virtual void PostAction(ERequestType pRequestType, EActionType pAccessType, string key = null, T model = null, string context = null) { }
        public virtual bool AuthorizeAction(ERequestType pRequestType, EActionType pAccessType, string key, ref T model, string context) { return true; }

        public virtual object InternalPostGet(T source) { return source; }

        public virtual QueryModifier BeforeFetch(EActionType read, EActionScope scope, QueryModifier modifier) { return null; }

        public virtual QueryModifier BeforeCount(EActionType read, EActionScope scope, QueryModifier modifier) { return null; }

        private IEnumerable<T> AfterFetch(EActionType read, EActionScope collection, IEnumerable<T> tempCollection) { return null; }

        #endregion

        #region HTTP Methods

        [HttpGet("")]
        public IActionResult WebAll()
        {
            var parsedModifier = Request.Query.ToQueryModifier();

            EvaluateAuthorization(ERequestType.Get, EActionType.Read, EActionScope.Collection);

            // A GetAll operation is just a query without filters, but sometimes we need something more.
            // So let's give the user the possibility by adding a hook to a modifier pre-processor.

            var modifier = BeforeFetch(EActionType.Read, EActionScope.Collection, parsedModifier) ?? parsedModifier;

            var tempCollection = Data<T>.Query(modifier);

            return PrepareResponse(tempCollection);
        }

        [HttpGet("{key}")]
        public virtual ActionResult<T> WebApiGet(string key)
        {
            try
            {
                EvaluateAuthorization(ERequestType.Get, EActionType.Read, EActionScope.Model, key);

                var preRet = Data<T>.Get(key);
                if (preRet == null) return NotFound();
                PostAction(ERequestType.Get, EActionType.Read, key, preRet);
                return new ActionResult<T>(preRet);
            }
            catch (Exception e)
            {
                Current.Log.Warn<T>($"GET {key}: {e.Message}");
                Current.Log.Add<T>(e);
                throw;
            }
        }

        #endregion
    }
}