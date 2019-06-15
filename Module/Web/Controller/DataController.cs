using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using Microsoft.AspNetCore.Mvc;
using Zen.Base;
using Zen.Base.Extension;
using Zen.Base.Module;
using Zen.Base.Module.Identity;
using Zen.Base.Module.Log;

namespace Zen.Module.Web.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class DataController<T> : Microsoft.AspNetCore.Mvc.Controller where T : Data<T>
    {
        public EndpointSetupAttribute Setup =>
            (EndpointSetupAttribute) Attribute.GetCustomAttribute(GetType(), typeof(EndpointSetupAttribute));


        private void EvaluateAuthorization(ERequestType requestType, EAccessType accessType, string identifier = null,
            T model = null, string parm3 = null)
        {
            var attr = Setup;

            var ret = true;
            if (identifier == null) identifier = model != null ? Data<T>.GetIdentifier(model) : "";

            if (attr != null)
            {
                var targetPermissionSet = "";

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
                }

                ret = IdentityHelper.HasAnyPermissions(targetPermissionSet);
            }

            if (ret)
                try
                {
                    ret = AuthorizeAction(requestType, accessType, identifier, ref model, parm3);
                }
                catch (Exception e) // User may throw a custom error, and that's fine: let's just log it.
                {
                    Current.Log.Add(
                        $"AUTH {typeof(T).FullName} DENIED {requestType}({accessType}) [{identifier}]. Reason: {e.Message}",
                        Message.EContentType.Warning);
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

            return new Dictionary<string, object> {{"x-zen-access", ret}};
        }

        public virtual bool AuthorizeAction(ERequestType pRequestType, EAccessType pAccessType, string identifier,
            ref T model, string pContext)
        {
            return true;
        }

        public virtual void PostAction(ERequestType pRequestType, EAccessType pAccessType, string pidentifier = null,
            T model = null, string pContext = null)
        {
        }

        public virtual object InternalPostGet(T source)
        {
            return source;
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
    }
}