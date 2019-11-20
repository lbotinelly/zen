using System;
using Microsoft.AspNetCore.Mvc;
using Zen.Web.Common.Results;

namespace Zen.Web.Communication.Push
{
    [Route("stack/communication/push")]
    public class PushController : ControllerBase
    {
        [Route("resources"), HttpGet]
        public object GetResources()
        {
            var list = GetType().Assembly.GetManifestResourceNames();
            return list;
        }

        [Route("resources/pushServiceWorker"), HttpGet]
        public virtual IActionResult GetPushServiceWorker()
        {
            Response.Headers.Add("Service-Worker-Allowed", "/");
            return new JavaScriptResult(Resources.pushServiceWorker);
        }

        [Route("register"), HttpPost]
        public virtual object DoRegister([FromBody] EndpointEntry ep)
        {
            try
            {
                Current.PushDispatcher.Register(ep);
                return true;
            } catch (Exception e)
            {
                Base.Current.Log.Add(e);
                return false;
            }
        }

        [Route("deregister"), HttpPost]
        public virtual object DoDeregister([FromBody] EndpointEntry ep)
        {
            try
            {
                Current.PushDispatcher.Deregister(ep);
                // Current.Log.Add(ep.ToJson());
                return true;
            } catch (Exception e)
            {
                Base.Current.Log.Add(e);
                return false;
            }
        }
    }
}