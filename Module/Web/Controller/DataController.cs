using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Zen.Base;
using Zen.Base.Module;
using Zen.Base.Module.Identity;
using Zen.Base.Module.Log;

// ReSharper disable InconsistentlySynchronizedField
// ReSharper disable StaticMemberInGenericType
// ReSharper disable InconsistentNaming

namespace Zen.Module.Web.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class DataController<T> : Microsoft.AspNetCore.Mvc.Controller where T : Data<T>
    {
        // ReSharper disable once StaticMemberInGenericType
        // ReSharper disable once InconsistentNaming
        private static readonly ConcurrentDictionary<Type, EndpointConfiguration> _attributeResolutionCache = new ConcurrentDictionary<Type, EndpointConfiguration>();

        private static readonly object _lockObject = new object();

        public EndpointConfiguration Setup
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
                        Security = (EndpointConfiguration.SecurityAttribute)Attribute.GetCustomAttribute(typeRef,
                            typeof(EndpointConfiguration.SecurityAttribute)),
                        Behavior = (EndpointConfiguration.BehaviorAttribute)Attribute.GetCustomAttribute(typeRef,
                            typeof(EndpointConfiguration.BehaviorAttribute))
                    };

                    _attributeResolutionCache.TryAdd(typeRef, newDefinition);

                    return newDefinition;
                }
            }
        }


        private void EvaluateAuthorization(ERequestType requestType, EAccessType accessType, string identifier = null, T model = null, string context = null)
        {
            var attr = Setup;

            var ret = true;
            if (identifier == null) identifier = model != null ? Data<T>.GetDataKey(model) : "";

            if (attr != null)
            {
                string targetPermissionSet;

                switch (accessType)
                {
                    case EAccessType.Read:
                        targetPermissionSet = attr.Security.Read;
                        break;
                    case EAccessType.Write:
                        targetPermissionSet = attr.Security.Write;
                        break;
                    case EAccessType.Remove:
                        targetPermissionSet = attr.Security.Remove;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(accessType), accessType, null);
                }

                ret = IdentityHelper.HasAnyPermissions(targetPermissionSet);
            }

            if (ret)
                try
                {
                    ret = AuthorizeAction(requestType, accessType, identifier, ref model, context);
                }
                catch (Exception e) // User may throw a custom error, and that's fine: let's just log it.
                {
                    Current.Log.Warn<T>($"AUTH DENIED {requestType} ({accessType}) [{identifier}]. Reason: {e.Message}");
                    throw new UnauthorizedAccessException("Not authorized: " + e.Message);
                }

            if (ret) return;

            Current.Log.Add(
                "Auth " + typeof(T).FullName + " DENIED " + requestType + "(" + accessType + ") [" + identifier + "]",
                Message.EContentType.Warning);

            throw new UnauthorizedAccessException("Not authorized.");
        }

        private Dictionary<string, object> GetAccessHeadersByPermission()
        {
            var ret = new Dictionary<string, object>
            {
                {"read", Setup.Security == null || IdentityHelper.HasAnyPermissions(Setup.Security.Read)},
                {"write", Setup.Security == null || IdentityHelper.HasAnyPermissions(Setup.Security.Write)},
                {"remove", Setup.Security == null || IdentityHelper.HasAnyPermissions(Setup.Security.Remove)}
            };

            return new Dictionary<string, object> { { "x-zen-access", ret } };
        }


        #region HTTP Methods

        [HttpGet("")]
        public virtual IEnumerable<T> WebApiGetAll()
        {
            return Data<T>.All();
        }

        [HttpGet("{identifier}")]
        public virtual ActionResult<T> WebApiGet(string identifier)
        {
            var sw = new Stopwatch();

            try
            {
                sw.Start();

                EvaluateAuthorization(ERequestType.Get, EAccessType.Read, identifier);

                var preRet = Data<T>.Get(identifier);

                if (preRet == null) return NotFound();

                Current.Log.Debug<T>($"GET {identifier} OK ({sw.ElapsedMilliseconds} ms)");

                PostAction(ERequestType.Get, EAccessType.Read, identifier, preRet);


                return new ActionResult<T>(preRet);
            }
            catch (Exception e)
            {
                sw.Stop();
                Current.Log.Warn<T>("GET " + identifier + " ERR (" + sw.ElapsedMilliseconds + " ms): " + e.Message);
                Current.Log.Add<T>(e);

                throw;
            }
        }

        #endregion

        #region virtual hooks
        public virtual void PostAction(ERequestType pRequestType, EAccessType pAccessType, string identifier = null, T model = null, string context = null) { }

        public virtual bool AuthorizeAction(ERequestType pRequestType, EAccessType pAccessType, string identifier, ref T model, string context) { return true; }


        public virtual object InternalPostGet(T source) { return source; }


        #endregion
    }
}