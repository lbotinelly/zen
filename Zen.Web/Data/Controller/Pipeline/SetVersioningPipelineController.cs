using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Zen.App.Data.Pipeline.ModelVersioning;
using Zen.App.Data.Pipeline.SetVersioning;
using Zen.Base;
using Zen.Base.Extension;
using Zen.Base.Module;
using Zen.Web.Data.Controller.Attributes;

namespace Zen.Web.Data.Controller.Pipeline
{
    public class SetVersioningPipelineController<T> : Microsoft.AspNetCore.Mvc.Controller where T : Data<T>
    {
        public DataBehavior ClassBehavior => (DataBehavior)Attribute.GetCustomAttribute(GetType(), typeof(DataBehavior));
        

        //private static long InternalCount()
        //{
        //    var DataSetVersion = (DataSetVersion)Attribute.GetCustomAttribute(typeof(T), typeof(DataSetVersion));

        //    if (Zen.App.Current.Orchestrator?.Person?.Locator == null) return 0;

        //    long preRet;

        //    if (DataSetVersion.CanBrowse()) { preRet = SetVersion<T>.Count(); }
        //    else
        //    {
        //        var q = new { OperatorLocator = Zen.App.Current.Orchestrator?.Person?.Locator }.ToJson();
        //        preRet = SetVersion<T>.Query(q).Count();
        //    }

        //    return preRet;
        //}

        //private IEnumerable<SetVersion<T>> InternalGetAllItems()
        //{
        //    var preRet = new List<SetVersion<T>>();

        //    if (Zen.App.Current.Orchestrator?.Person?.Locator == null) return preRet;

        //    var hs = Helper.Setup<T>();

        //    Log.Add("SetVersion InternalGetAllItems > IsSetVersioningOperator? > " + hs.ToJson());

        //    if (hs.IsSetVersioningOperator())
        //    {
        //        preRet = SetVersion<T>.Get().ToList();
        //        preRet.Add(new SetVersion<T>
        //        {
        //            Code = Constants.CURRENT_LIVE_WORKSET_TAG,
        //            Id = Constants.CURRENT_LIVE_WORKSET_TAG,
        //            Name = "Editorial Workset",
        //            IsPublic = true,
        //            IsLocked = false,
        //            IsCurrent = !preRet.Any(i => i.IsCurrent)
        //        });
        //    }
        //    else
        //    {
        //        // var q = new {OperatorLocator = Current.Orchestrator?.Person?.Locator}.ToJson();
        //        var q = new { IsPublic = true }.ToJson();

        //        preRet = SetVersion<T>.Query(q).ToList();
        //    }

        //    preRet = preRet.OrderBy(i => i.TimeStamp).Reverse().ToList();

        //    return preRet;
        //}

        //#region download / upload

        //[Route("version/download"), HttpGet]
        //public virtual HttpResponseMessage WebApiDownloadWorkspace() { return GetZipPackage(); }

        //[Route("version/upload"), HttpPost]
        //public virtual HttpResponseMessage WebApiUploadToWorkspace()
        //{
        //    // https://stackoverflow.com/questions/33387764/how-to-post-file-to-asp-net-web-api-2
        //    // https://stackoverflow.com/a/33388882/1845714

        //    var httpRequest = HttpContext.Current.Request;

        //    try
        //    {
        //        if (httpRequest.Files.Count > 0)
        //        {
        //            foreach (string fileName in httpRequest.Files.Keys)
        //            {
        //                var file = httpRequest.Files[fileName];

        //                //https://stackoverflow.com/questions/10344449/what-is-the-shortest-way-to-get-the-string-content-of-a-httppostedfile-in-c-shar
        //                if (file == null) continue;

        //                var str = new StreamReader(file.InputStream).ReadToEnd();
        //                var objs = str.FromJson<List<T>>();

        //                MicroEntity<T>.Save(objs);
        //            }

        //            return Request.CreateResponse(HttpStatusCode.Accepted);
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        Log.Add(e);
        //        throw;
        //    }

        //    return Request.CreateResponse(HttpStatusCode.BadRequest);
        //}

        //[Route("version/download/{id}"), HttpGet]
        //public virtual HttpResponseMessage WebApiDownloadWorkspace(string id) { return GetZipPackage(id); }

        //private static HttpResponseMessage GetZipPackage(string id = null)
        //{
        //    var hs = Helper.Setup<T>();
        //    if (!hs.IsSetVersioningOperator()) return new HttpResponseMessage(HttpStatusCode.Forbidden) { Content = new StringContent("User is not a Set Version Operator.") };

        //    var httpResponseMessage = new HttpResponseMessage();

        //    var sw = new Stopwatch();

        //    try
        //    {
        //        sw.Start();

        //        var fullName = Status.Application.Code + "-" + typeof(T).Name;
        //        fullName += id != null ? "-" + id : "";

        //        var package = SetVersion<T>.GetPackage(id);

        //        using (var memoryStream = new MemoryStream())
        //        {
        //            using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
        //            using (var entryStream = archive.CreateEntry(fullName + ".json", CompressionLevel.Optimal).Open())
        //            using (var streamWriter = new StreamWriter(entryStream)) { streamWriter.Write(package.ToJson()); }

        //            memoryStream.Seek(0, SeekOrigin.Begin);

        //            httpResponseMessage.Content = new ByteArrayContent(memoryStream.ToArray());
        //            httpResponseMessage.Content.Headers.ContentType = new MediaTypeHeaderValue("application/zip");
        //            httpResponseMessage.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment") { FileName = fullName + ".zip" };
        //            httpResponseMessage.StatusCode = HttpStatusCode.OK;

        //            sw.Stop();
        //        }

        //        Log.Add($"GET {typeof(T).FullName}: SetVersioning DOWNLOAD Workspace OK ({sw.ElapsedMilliseconds} ms)");

        //        return httpResponseMessage;
        //    }
        //    catch (Exception e)
        //    {
        //        sw.Stop();
        //        Log.Add($"GET {typeof(T).FullName}: SetVersioning DOWNLOAD Workspace ERR ({sw.ElapsedMilliseconds} ms): {e.Message}", e);
        //        return new HttpResponseMessage(HttpStatusCode.InternalServerError) { Content = new StringContent(e.FancyString()) };
        //    }
        //}

        //#endregion

        //#region Set management

        //[Route("version/set"), HttpGet]
        //public virtual object GetAllItems()
        //{
        //    var sw = new Stopwatch();

        //    try
        //    {
        //        sw.Start();
        //        var preRet = InternalGetAllItems();
        //        sw.Stop();

        //        Log.Add("GET " + typeof(T).FullName + ": GetAllItems OK (" + sw.ElapsedMilliseconds + " ms)");
        //        return ControllerHelper.RenderJsonResult(preRet);
        //    }
        //    catch (Exception e)
        //    {
        //        sw.Stop();
        //        Log.Add("GET " + typeof(T).FullName + ": GetAllItems ERR (" + sw.ElapsedMilliseconds + " ms): " + e.Message, e);
        //        return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, e);
        //    }
        //}

        //[Route("version/set"), HttpPost]
        //public virtual object PostVersionSet(SetVersion<T> item)
        //{
        //    var sw = new Stopwatch();

        //    var hs = Helper.Setup<T>();
        //    if (!hs.IsSetVersioningOperator()) return new HttpResponseMessage(HttpStatusCode.Forbidden) { Content = new StringContent("User is not a Set Version Operator.") };

        //    try
        //    {
        //        sw.Start();
        //        var preRet = SetVersion<T>.Get(item.Save());
        //        sw.Stop();

        //        Log.Add("POST " + typeof(T).FullName + ": PostVersionSet OK (" + sw.ElapsedMilliseconds + " ms)");
        //        return ControllerHelper.RenderJsonResult(preRet);
        //    }
        //    catch (Exception e)
        //    {
        //        sw.Stop();
        //        Log.Add("POST " + typeof(T).FullName + ": PostVersionSet ERR (" + sw.ElapsedMilliseconds + " ms): " + e.Message, e);
        //        return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, e);
        //    }
        //}

        //[Route("version/set/status"), HttpGet]
        //public virtual object GetStatus()
        //{
        //    var preret = new { count = InternalCount() };
        //    var ret = ControllerHelper.RenderJsonResult(preret);
        //    ControllerHelper.ProcessPipelineHeaders<T>(ret.Headers);
        //    return ret;
        //}

        //[Route("version/set/{id}"), HttpGet]
        //public virtual object GetTaskById(string id)
        //{
        //    var sw = new Stopwatch();

        //    try
        //    {
        //        sw.Start();
        //        var preRet = SetVersion<T>.Get(id);
        //        sw.Stop();

        //        Log.Add("GET " + typeof(T).FullName + ": SetVersioning GET OK (" + sw.ElapsedMilliseconds + " ms)");
        //        return ControllerHelper.RenderJsonResult(preRet);
        //    }
        //    catch (Exception e)
        //    {
        //        sw.Stop();
        //        Log.Add(
        //            "GET " + typeof(T).FullName + ": SetVersioning GET ERR (" + sw.ElapsedMilliseconds + " ms): " + e.Message,
        //            e);
        //        return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, e);
        //    }
        //}

        //[Route("version/set/{id}/current"), HttpGet]
        //public virtual HttpResponseMessage WebApiSetCurrent(string id)
        //{
        //    var hs = Helper.Setup<T>();
        //    if (!hs.IsSetVersioningOperator()) return Request.CreateErrorResponse(HttpStatusCode.Forbidden, "User is not a Set Version Operator.");

        //    var sw = new Stopwatch();

        //    try
        //    {
        //        sw.Start();

        //        var probe = SetVersion<T>.Get(id);
        //        if (probe == null) return Request.CreateErrorResponse(HttpStatusCode.Forbidden, "Invalid ID.");

        //        if (probe.IsCurrent) return ControllerHelper.RenderJsonResult(probe);

        //        var allRecs = SetVersion<T>.GetAll();
        //        foreach (var setVersion in allRecs)
        //        {
        //            if (!setVersion.IsCurrent) continue;

        //            setVersion.IsCurrent = false;
        //            setVersion.Save();
        //        }

        //        probe.IsCurrent = true;
        //        probe = SetVersion<T>.Get(probe.Save());

        //        return ControllerHelper.RenderJsonResult(probe);
        //    }
        //    catch (Exception e)
        //    {
        //        sw.Stop();
        //        Log.Add($"WebApiSetCurrent {typeof(T).FullName}: [{id}] ERR ({sw.ElapsedMilliseconds} ms): {e.Message}", e);
        //        return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, e);
        //    }
        //}

        //[Route("version/set/{id}/pull"), HttpGet]
        //public virtual HttpResponseMessage WebApiPullToWorkspace(string id)
        //{
        //    var hs = Helper.Setup<T>();
        //    if (!hs.IsSetVersioningOperator()) return Request.CreateErrorResponse(HttpStatusCode.Forbidden, "User is not a Set Version Operator.");

        //    var probe = SetVersion<T>.Get(id);
        //    if (probe == null) return Request.CreateErrorResponse(HttpStatusCode.NotFound, $"Version {id} not found.");

        //    var sw = new Stopwatch();

        //    try
        //    {
        //        sw.Start();
        //        probe.PullToWorkspace();
        //        sw.Stop();

        //        Log.Add($"GET {typeof(T).FullName}: SetVersioning PULL ID [{id}] OK ({sw.ElapsedMilliseconds} ms)");

        //        return ControllerHelper.RenderJsonResult(probe);
        //    }
        //    catch (Exception e)
        //    {
        //        sw.Stop();
        //        Log.Add($"GET {typeof(T).FullName}: SetVersioning PULL ID [{id}] ERR ({sw.ElapsedMilliseconds} ms): {e.Message}", e);
        //        return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, e);
        //    }
        //}

        //[Route("version/set/{id}/push"), HttpGet]
        //public virtual HttpResponseMessage WebApiPushFromWorkspace(string id)
        //{
        //    var hs = Helper.Setup<T>();
        //    if (!hs.IsSetVersioningOperator()) return Request.CreateErrorResponse(HttpStatusCode.Forbidden, "User is not a Set Version Operator.");

        //    var sw = new Stopwatch();

        //    try
        //    {
        //        sw.Start();
        //        var probe = SetVersion<T>.PushFromWorkspace(id);
        //        sw.Stop();
        //        Log.Add($"GET {typeof(T).FullName}: SetVersioning PUSH ID [{id}] OK ({sw.ElapsedMilliseconds} ms)");
        //        return ControllerHelper.RenderJsonResult(probe);
        //    }
        //    catch (Exception e)
        //    {
        //        sw.Stop();
        //        Log.Add($"GET {typeof(T).FullName}: SetVersioning PUSH ID [{id}] ERR ({sw.ElapsedMilliseconds} ms): {e.Message}", e);
        //        return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, e);
        //    }
        //}

        //[Route("version/set/{tid}/drop"), HttpGet]
        //public virtual HttpResponseMessage WebApiDrop(string tid)
        //{
        //    var hs = Helper.Setup<T>();
        //    if (!hs.IsSetVersioningOperator()) return Request.CreateErrorResponse(HttpStatusCode.Forbidden, "User is not a Set Version Operator.");

        //    var probe = SetVersion<T>.Get(tid);
        //    if (probe == null) return Request.CreateErrorResponse(HttpStatusCode.NotFound, $"Version {tid} not found.");

        //    var sw = new Stopwatch();

        //    try
        //    {
        //        sw.Start();
        //        probe.Remove();
        //        sw.Stop();

        //        Log.Add($"GET {typeof(T).FullName}: SetVersioning DROP ID [{tid}] OK ({sw.ElapsedMilliseconds} ms)");

        //        return ControllerHelper.RenderJsonResult(probe);
        //    }
        //    catch (Exception e)
        //    {
        //        sw.Stop();
        //        Log.Add($"GET {typeof(T).FullName}: SetVersioning DROP ID [{tid}] ERR ({sw.ElapsedMilliseconds} ms): {e.Message}", e);
        //        return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, e);
        //    }
        //}

        //[Route("version/set/{id}/toggle/public"), HttpGet]
        //public virtual HttpResponseMessage WebApiTogglePublic(string id)
        //{
        //    var hs = Helper.Setup<T>();
        //    if (!hs.IsSetVersioningOperator()) return Request.CreateErrorResponse(HttpStatusCode.Forbidden, "User is not a Set Version Operator.");

        //    var sw = new Stopwatch();

        //    try
        //    {
        //        sw.Start();

        //        var probe = SetVersion<T>.Get(id);
        //        if (probe == null) return Request.CreateErrorResponse(HttpStatusCode.Forbidden, "Invalid ID.");

        //        probe.IsPublic = !probe.IsPublic;
        //        probe.Save();
        //        probe = SetVersion<T>.Get(probe.Save());

        //        return ControllerHelper.RenderJsonResult(probe);
        //    }
        //    catch (Exception e)
        //    {
        //        sw.Stop();
        //        Log.Add($"WebApiTogglePublic {typeof(T).FullName}: [{id}] ERR ({sw.ElapsedMilliseconds} ms): {e.Message}", e);
        //        return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, e);
        //    }
        //}

        //[Route("version/set/{id}/toggle/locked"), HttpGet]
        //public virtual HttpResponseMessage WebApiToggleLocked(string id)
        //{
        //    var hs = Helper.Setup<T>();
        //    if (!hs.IsSetVersioningOperator()) return Request.CreateErrorResponse(HttpStatusCode.Forbidden, "User is not a Set Version Operator.");

        //    var sw = new Stopwatch();

        //    try
        //    {
        //        sw.Start();

        //        var probe = SetVersion<T>.Get(id);
        //        if (probe == null) return Request.CreateErrorResponse(HttpStatusCode.Forbidden, "Invalid ID.");

        //        probe.IsLocked = !probe.IsLocked;
        //        probe.Save();
        //        probe = SetVersion<T>.Get(probe.Save());

        //        return ControllerHelper.RenderJsonResult(probe);
        //    }
        //    catch (Exception e)
        //    {
        //        sw.Stop();
        //        Log.Add($"WebApiTogglePublic {typeof(T).FullName}: [{id}] ERR ({sw.ElapsedMilliseconds} ms): {e.Message}", e);
        //        return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, e);
        //    }
        //}

        //#endregion

        //#region Set Access

        //[Route("version/{code}"), HttpGet]
        //public virtual HttpResponseMessage WebApiSetVersioningGetAll(string code)
        //{
        //    var hs = Helper.Setup<T>();
        //    if (!hs.IsSetVersioningOperator()) return Request.CreateErrorResponse(HttpStatusCode.Forbidden, "User is not a Set Version Operator.");

        //    var sw = new Stopwatch();

        //    try
        //    {
        //        sw.Start();

        //        var probe = SetVersion<T>.GetByCode(code);
        //        if (probe == null) return Request.CreateErrorResponse(HttpStatusCode.Forbidden, "Invalid Code.");

        //        if (ClassBehavior.CacheResults)
        //        {
        //            var cached = LocalCache.Get(probe.GetItemCacheKey());

        //            if (cached != null)
        //            {
        //                var cacheRet = ControllerHelper.RenderStringResult(cached);
        //                cacheRet.Headers.CacheControl = new CacheControlHeaderValue { Public = true, MaxAge = new TimeSpan(1, 0, 0, 0) }; // Cache it for 1 day
        //                return cacheRet;
        //            }
        //        }

        //        var tempPackage = probe.GetPackage();

        //        var ret = tempPackage.Items;

        //        if (ClassBehavior.SummaryType != null) ret = (IEnumerable<object>)ret.ToJson().FromJson(ClassBehavior.SummaryType, true);

        //        if (ClassBehavior.CacheResults) LocalCache.Put(probe.GetItemCacheKey(), ret.ToJson());

        //        return ControllerHelper.RenderJsonResult(ret);
        //    }
        //    catch (Exception e)
        //    {
        //        sw.Stop();
        //        Log.Add($"WebApiTogglePublic {typeof(T).FullName}: [{code}] ERR ({sw.ElapsedMilliseconds} ms): {e.Message}", e);
        //        return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, e);
        //    }
        //}

        //[Route("version/{code}/{locator}"), HttpGet]
        //public virtual HttpResponseMessage WebApiSetVersioningGetEntry(string code, string locator)
        //{
        //    var hs = Helper.Setup<T>();
        //    if (!hs.IsSetVersioningOperator()) return Request.CreateErrorResponse(HttpStatusCode.Forbidden, "User is not a Set Version Operator.");

        //    var sw = new Stopwatch();

        //    try
        //    {
        //        sw.Start();

        //        var probe = SetVersion<T>.GetByCode(code);

        //        if (probe == null) return Request.CreateErrorResponse(HttpStatusCode.Forbidden, "Invalid Code.");

        //        var preRet = probe.VersionGetItem(locator);
        //        var postRet = InternalPostGet(preRet, code, locator);

        //        return ControllerHelper.RenderJsonResult(postRet);
        //    }
        //    catch (Exception e)
        //    {
        //        sw.Stop();
        //        Log.Add($"WebApiTogglePublic {typeof(T).FullName}: [{code}] ERR ({sw.ElapsedMilliseconds} ms): {e.Message}", e);
        //        return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, e);
        //    }
        //}

        //public virtual object InternalPostGet(T source, string code, string locator) { return source; }

        //#endregion
    }
}
