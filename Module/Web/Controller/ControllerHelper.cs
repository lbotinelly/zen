using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using Zen.Base;
using Zen.Base.Extension;
using Zen.Base.Module;
using Zen.Base.Module.Log;

namespace Zen.Module.Web.Controller
{
    public static class ControllerHelper
    {
        internal static void HandleHeaders(HttpHeaders retHeaders, ConcurrentDictionary<string, object> hs)
        {
            if (hs == null) return;

            foreach (var j in hs)
            {
                if (retHeaders.Contains(j.Key)) retHeaders.Remove(j.Key);
                retHeaders.Add(j.Key, j.Value.ToJson());
            }
        }
        public static void ProcessPipelineHeaders<T>(HttpHeaders retHeaders) where T : Data<T>
        {

            if (Data<T>.Info<T>.Settings?.Pipelines?.Before != null)
                foreach (var i in Data<T>.Info<T>.Settings.Pipelines.Before) HandleHeaders(retHeaders, i.Headers<T>());

            if (Data<T>.Info<T>.Settings?.Pipelines?.After != null)
                foreach (var i in Data<T>.Info<T>.Settings.Pipelines.After) HandleHeaders(retHeaders, i.Headers<T>());
        }

        public static HttpResponseMessage RenderJsonResult(object contents)
        {
            try
            {
                var ret = new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(contents.ToJson()) };
                ret.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                return ret;
            }
            catch (Exception e)
            {
                Current.Log.Add(e, "RenderJsonResult ERR: ");
                Current.Log.Add("RenderJsonResult" + contents.ToJson(), Message.EContentType.Info);
                throw;
            }
        }

        public static HttpResponseMessage RenderStringResult(string contents)
        {
            try
            {
                var ret = new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(contents) };
                ret.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                return ret;
            }
            catch (Exception e)
            {
                Current.Log.Add(e, "RenderStringResult ERR: ");
                Current.Log.Add("RenderStringResult" + contents.ToJson(), Message.EContentType.Info);
                throw;
            }
        }
    }
}