using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Zen.App.Data.Log;
using Zen.App.Data.Pipeline.SetVersioning;
using Zen.Base;
using Zen.Base.Extension;
using Zen.Base.Module;
using Constants = Zen.App.Data.Pipeline.SetVersioning.Constants;

namespace Zen.Web.App.Data.Controller.Pipeline.SetVersioning
{
    public class SetVersioningController<T> : ControllerBase where T : Data<T>
    {
        private static long Count()
        {
            var dataSetVersion = (SetVersioningPrimitiveAttribute)Attribute.GetCustomAttribute(typeof(T), typeof(SetVersioningPrimitiveAttribute));

            if (Zen.App.Current.Orchestrator?.Person?.Locator == null) return 0;

            long preRet;

            if (dataSetVersion.CanBrowse()) { preRet = SetVersion<T>.Count(); }
            else
            {
                var personLocator = Zen.App.Current.Orchestrator?.Person?.Locator;

                preRet = SetVersion<T>.Where(i => i.OperatorLocator == personLocator).Count();
            }

            return preRet;
        }

        private static IEnumerable<SetVersion<T>> InternalGetAllItems()
        {
            var preRet = new List<SetVersion<T>>();

            if (Zen.App.Current.Orchestrator?.Person?.Locator == null) return preRet;

            if (SetVersion<T>.CanBrowse())
            {
                preRet = SetVersion<T>.All().ToList();

                if (SetVersion<T>.CanModify())
                    preRet.Add(new SetVersion<T>
                    {
                        Code = Constants.CURRENT_LIVE_WORKSET_TAG,
                        Id = Constants.CURRENT_LIVE_WORKSET_TAG,
                        Name = "Workset",
                        IsPublic = true,
                        IsLocked = false,
                        IsCurrent = !preRet.Any(i => i.IsCurrent)
                    });
            }
            else { preRet = SetVersion<T>.Where(i => i.IsPublic).ToList(); }

            preRet = preRet.OrderBy(i => i.TimeStamp).Reverse().ToList();

            return preRet;
        }

        #region Set Access

        //[Route("version/{code}"), HttpGet]
        //public virtual object WebApiSetVersioningGetAll(string code)
        //{
        //    if (!SetVersion<T>.CanModify()) throw new AuthenticationException("User is not a Set Version Operator.");

        //    var sw = new Stopwatch();

        //    try
        //    {
        //        sw.Start();

        //        var probe = SetVersion<T>.GetByCode(code);
        //        if (probe == null) throw new InvalidDataException("Invalid ID.");

        //        var tempPackage = probe.GetPackage();

        //        var ret = tempPackage.Items;

        //        if (ClassBehavior.SummaryType!= null) ret = (IEnumerable<object>) ret.ToJson().FromJson(ClassBehavior.SummaryType, true);

        //        if (ClassBehavior.CacheResults)

        //        return ret;
        //    } catch (Exception e)
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
        //    } catch (Exception e)
        //    {
        //        sw.Stop();
        //        Log.Add($"WebApiTogglePublic {typeof(T).FullName}: [{code}] ERR ({sw.ElapsedMilliseconds} ms): {e.Message}", e);
        //        return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, e);
        //    }
        //}

        [NonAction]
        public virtual object InternalPostGet(T source, string code, string locator) { return source; }

        #endregion

        #region download/upload

        public class PostPayload
        {
            public IFormFile file { get; set; }
            // Other properties
        }

        [Route("version/upload"), HttpPost]
        public IActionResult UploadToWorkspace(PostPayload upload)
        {
            if (!SetVersion<T>.CanModify()) throw new AuthenticationException("User is not a Set Version Operator.");

            var formFile = upload.file;

            try
            {
                var size = formFile.Length;

                var filePaths = new List<string>();

                if (formFile.Length <= 0) return null;

                // full path to file in temp location
                var filePath = Path.GetTempFileName();
                filePaths.Add(filePath);

                var str = new StreamReader(formFile.OpenReadStream()).ReadToEnd();
                var packageModel = str.FromJson<SetVersion<T>.Payload>();
                var objs = packageModel.Items.ToJson().FromJson<List<T>>();

                Data<T>.RemoveAll();
                Data<T>.Save(objs, null, true);

                Data<T>.New().AfterSetUpdate();

                return Ok(new { size, filePaths });
            }
            catch (Exception e)
            {
                Log.Add(e);
                throw;
            }
        }

        [Route("version/download"), HttpGet]
        public virtual IActionResult WebApiDownloadWorkspace() { return WebApiDownloadWorkspace(null); }

        [Route("version/download/{code}"), HttpGet]
        public virtual IActionResult WebApiDownloadWorkspace(string code)
        {
            if (!SetVersion<T>.CanBrowse()) throw new AuthenticationException("User is not authorized to download.");

            return GetZipPackage(code);
        }

        public class ZipDescriptor
        {
            public string Name;
            public Stream Stream;
        }

        private FileContentResult GetZipPackage(string code = null)
        {
            if (!SetVersion<T>.CanModify()) throw new AuthenticationException("User is not a Set Version Operator.");

            var sw = new Stopwatch();

            try
            {
                sw.Start();

                var fullName = $"{Zen.App.Current.Orchestrator.Application.Code}.{typeof(T).Name}{(code != null ? $".{code}" : "")}";

                var package = SetVersion<T>.GetPackage(code);

                byte[] bytes;

                using (var memoryStream = new MemoryStream())
                {
                    using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
                    {
                        using (var zipEntry = archive.CreateEntry($"{fullName}.json", CompressionLevel.Optimal).Open())
                        using (var zipWriter = new StreamWriter(zipEntry))
                        {
                            zipWriter.Write(package.ToJson());
                            zipWriter.Flush();
                        }

                        memoryStream.Seek(0, SeekOrigin.Begin);
                        memoryStream.Position = 0;
                        bytes = memoryStream.ToArray();
                        // memoryStream.Seek(0, SeekOrigin.End);
                    }
                }

                sw.Stop();
                Log.Add<T>($"GET: SetVersioning DOWNLOAD {fullName} OK - {sw.ElapsedMilliseconds} ms, {bytes.Length.ToByteSize()}");

                var person = Zen.App.Current.Orchestrator.Person;

                new Log<T>
                {
                    ReferenceId = package.Descriptor.Id,
                    AuthorLocator = person?.Locator,
                    Action = "DOWNLOAD",
                    Type = Zen.App.Data.Log.Constants.Type.VERSIONING,
                    Message = $"Version [{package.Descriptor.Code}] ({package.Descriptor.Name}) download{(person != null ? $" by [{person.Locator}] {person.Name}" : "")}"
                }.Insert();

                return File(bytes, "application/zip", fullName + ".zip", true);
            }
            catch (Exception e)
            {
                sw.Stop();
                Log.Add<T>(e, $"GET: SetVersioning DOWNLOAD {code} ERR ({sw.ElapsedMilliseconds} ms): {e.Message}");
                throw;
            }
        }

        #endregion

        #region Set management

        [Route("version/set"), HttpGet]
        public virtual object GetAll()
        {
            var sw = new Stopwatch();

            try
            {
                sw.Start();
                var preRet = InternalGetAllItems();
                sw.Stop();

                Log.Add<T>("SetVersion GetAll OK (" + sw.ElapsedMilliseconds + " ms)");
                return preRet;
            }
            catch (Exception e)
            {
                sw.Stop();
                Log.Add<T>(e, "SetVersion GetAll ERR (" + sw.ElapsedMilliseconds + " ms): " + e.Message);
                throw;
            }
        }

        [Route("version/set"), HttpPost]
        public virtual object PostItem(SetVersion<T> item)
        {
            var sw = new Stopwatch();

            if (!SetVersion<T>.CanModify()) throw new AuthenticationException("User is not a Set Version Operator.");

            try
            {
                sw.Start();
                var preRet = SetVersion<T>.Get(item.Save().Id);
                sw.Stop();

                Log.Add<T>("SetVersion PostItem OK (" + sw.ElapsedMilliseconds + " ms)");
                return preRet;
            }
            catch (Exception e)
            {
                sw.Stop();
                Log.Add<T>(e, "SetVersion PostItem ERR (" + sw.ElapsedMilliseconds + " ms): " + e.Message);
                throw;
            }
        }

        [Route("version/set/status"), HttpGet]
        public virtual object GetStatus()
        {
            if (!SetVersion<T>.CanBrowse()) throw new AuthenticationException("User is not authorized to download.");

            var preret = new { count = Count() };
            return preret;
        }

        [Route("version/set/{id}"), HttpGet]
        public virtual object GetById(string id)
        {
            if (!SetVersion<T>.CanBrowse()) throw new AuthenticationException("User is not authorized to download.");

            var sw = new Stopwatch();

            try
            {
                sw.Start();
                var preRet = SetVersion<T>.Get(id);
                sw.Stop();

                Log.Add<T>($"SetVersion : GetById [{id}] OK ({sw.ElapsedMilliseconds} ms)");
                return preRet;
            }
            catch (Exception e)
            {
                sw.Stop();
                Log.Add<T>(e, $"SetVersion: GetById [{id}] ERR ({sw.ElapsedMilliseconds} ms)");
                throw;
            }
        }

        [Route("version/set/{id}/current"), HttpGet]
        public virtual object SetCurrent(string id)
        {
            if (!SetVersion<T>.CanBrowse()) throw new AuthenticationException("User is not authorized.");

            var sw = new Stopwatch();

            try
            {
                sw.Start();

                var probe = SetVersion<T>.Get(id);
                if (probe == null) throw new InvalidDataException("Invalid ID.");

                if (probe.IsCurrent) return probe;

                var allCurrentSets = SetVersion<T>.Where(i => i.IsCurrent);
                foreach (var setVersion in allCurrentSets)
                {
                    if (!setVersion.IsCurrent) continue;

                    setVersion.IsCurrent = false;
                    setVersion.Save();
                }

                probe.IsCurrent = true;
                probe = SetVersion<T>.Get(probe.Save().Id);

                sw.Stop();

                Log.Add<T>($"SetVersion : SetCurrent [{id}] OK ({sw.ElapsedMilliseconds} ms)");

                return probe;
            }
            catch (Exception e)
            {
                sw.Stop();
                Log.Add<T>(e, $"SetVersion: SetCurrent [{id}] ERR ({sw.ElapsedMilliseconds} ms): {e.Message}");
                throw;
            }
        }

        [Route("version/set/{id}/pull"), HttpGet]
        public virtual object PullToWorkset(string id)
        {
            if (!SetVersion<T>.CanModify()) throw new AuthenticationException("User is not a Set Version Operator.");

            var sw = new Stopwatch();

            try
            {
                var probe = SetVersion<T>.Get(id);
                if (probe == null) throw new InvalidDataException("Invalid ID.");

                sw.Start();
                probe.PullToWorkset();
                sw.Stop();

                Log.Add<T>($"SetVersion: PullToWorkset [{id}] OK ({sw.ElapsedMilliseconds} ms)");

                return probe;
            }
            catch (Exception e)
            {
                sw.Stop();
                Log.Add<T>(e, $"SetVersion: PullToWorkset [{id}] ERR ({sw.ElapsedMilliseconds} ms): {e.Message}");
                throw;
            }
        }

        [Route("version/set/{id}/push"), HttpGet]
        public virtual object PushFromWorkset(string id)
        {
            if (!SetVersion<T>.CanModify()) throw new AuthenticationException("User is not a Set Version Operator.");

            var sw = new Stopwatch();

            try
            {
                sw.Start();
                var probe = SetVersion<T>.PushFromWorkset(id);
                sw.Stop();
                Log.Add<T>($"SetVersion: PushFromWorkset [{id}] OK ({sw.ElapsedMilliseconds} ms)");
                return probe;
            }
            catch (Exception e)
            {
                sw.Stop();
                Log.Add<T>(e, $"SetVersion: PushFromWorkset [{id}] ERR ({sw.ElapsedMilliseconds} ms): {e.Message}");
                throw;
            }
        }

        [Route("version/set/{tid}/drop"), HttpGet]
        public virtual object DropSet(string tid)
        {
            if (!SetVersion<T>.CanModify()) throw new AuthenticationException("User is not a Set Version Operator.");

            var probe = SetVersion<T>.Get(tid);
            if (probe == null) throw new InvalidDataException("Invalid ID.");

            var sw = new Stopwatch();

            try
            {
                sw.Start();
                probe.Remove();
                sw.Stop();

                Log.Add<T>($"SetVersion: DropSet [{tid}] OK ({sw.ElapsedMilliseconds} ms)");

                return probe;
            }
            catch (Exception e)
            {
                sw.Stop();
                Log.Add<T>(e, $"SetVersion: DropSet [{tid}] ERR ({sw.ElapsedMilliseconds} ms): {e.Message}");
                throw;
            }
        }

        [Route("version/set/{id}/toggle/public"), HttpGet]
        public virtual object TogglePublic(string id)
        {
            if (!SetVersion<T>.CanModify()) throw new AuthenticationException("User is not a Set Version Operator.");

            var sw = new Stopwatch();

            try
            {
                sw.Start();

                var probe = SetVersion<T>.Get(id);
                if (probe == null) throw new InvalidDataException("Invalid ID.");

                probe.IsPublic = !probe.IsPublic;
                probe.Save();
                probe = SetVersion<T>.Get(probe.Save().Id);
                sw.Stop();

                Log.Add<T>($"SetVersion: TogglePublic [{id}] OK ({sw.ElapsedMilliseconds} ms)");

                return probe;
            }
            catch (Exception e)
            {
                sw.Stop();
                Log.Add<T>(e, $"SetVersion: TogglePublic [{id}] ERR ({sw.ElapsedMilliseconds} ms): {e.Message}");
                throw;
            }
        }

        [Route("version/set/{id}/toggle/locked"), HttpGet]
        public virtual object ToggleLocked(string id)
        {
            if (!SetVersion<T>.CanModify()) throw new AuthenticationException("User is not a Set Version Operator.");

            var sw = new Stopwatch();

            try
            {
                sw.Start();

                var probe = SetVersion<T>.Get(id);
                if (probe == null) throw new InvalidDataException("Invalid ID.");

                probe.IsLocked = !probe.IsLocked;
                probe.Save();
                probe = SetVersion<T>.Get(probe.Save().Id);
                sw.Stop();

                Log.Add<T>($"SetVersion: ToggleLocked [{id}] OK ({sw.ElapsedMilliseconds} ms)");

                return probe;
            }
            catch (Exception e)
            {
                sw.Stop();
                Log.Add<T>(e, $"SetVersion: ToggleLocked [{id}] ERR ({sw.ElapsedMilliseconds} ms): {e.Message}");
                throw;
            }
        }

        #endregion
    }
}