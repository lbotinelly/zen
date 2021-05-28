using System;
using System.Net;
using System.Net.Http;
using System.Security.Authentication;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Zen.Base.Extension;

namespace Zen.Web.App.Framework.Data
{
    [Route("framework/data")]
    public class PreferencesController : ControllerBase
    {
        [Route("preferences"), HttpGet]
        public object WebApiPreferencesGet()
        {
            if (Zen.App.Current.Orchestrator?.Person == null) throw new AuthenticationException("No signed-in user");

            return Zen.App.Current.Preference.GetCurrent().GetValues().ToJson();
        }

        [Route("preferences"), HttpPost]
        public HttpResponseMessage WebApiPreferencesPost(JObject opValue)
        {
            HttpResponseMessage ret;

            if (Zen.App.Current.Orchestrator?.Person == null) throw new AuthenticationException("No signed-in user");

            try
            {
                Zen.App.Current.Preference.Put(opValue);
                ret = new HttpResponseMessage(HttpStatusCode.OK);
            }
            catch (Exception e)
            {
                ret = new HttpResponseMessage(HttpStatusCode.InternalServerError)
                {
                    Content = new StringContent(e.Message)
                };
            }

            return ret;
        }
    }
}