using System;
using Zen.Base;
using Zen.Web.Communication.Push;

namespace Zen.Module.Cloud.Google.Communication.Push
{
    public abstract class GooglePushDispatcher : PushDispatcherPrimitive
    {
        public override void Send(EndpointEntry target, object obj)
        {
            try
            {
                var ret = Helper.SendNotification(target, obj);
                Current.Log.Add("GoogleDispatcher: " + (ret ? "OK  " : "FAIL") + " " + target.endpoint);
                HandlePushAttempt(target.endpoint, ret);
            } catch (Exception e) { Current.Log.Add(e); }
        }

        public class Payload
        {
            public string to { get; set; }
            public object data { get; set; }
        }
    }
}