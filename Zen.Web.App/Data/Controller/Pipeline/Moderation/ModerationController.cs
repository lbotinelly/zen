using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Zen.Base;
using Zen.Base.Extension;
using Zen.Base.Module;
using Zen.Base.Module.Data;
using Zen.Web.App.Data.Pipeline.Moderation;
using Zen.Web.Data.Controller;
using Zen.Web.Data.Controller.Attributes;

// ReSharper disable InconsistentlySynchronizedField
// ReSharper disable StaticMemberInGenericType
// ReSharper disable InconsistentNaming
namespace Zen.Web.App.Data.Controller.Pipeline.Moderation
{
    public class ModerationController<T> : ControllerBase where T : Data<T>
    {
        private static readonly ConcurrentDictionary<Type, EndpointConfiguration> _attributeResolutionCache = new ConcurrentDictionary<Type, EndpointConfiguration>();
        private static readonly object _lockObject = new object();
        private Mutator _mutator;

        protected bool CanRead => Configuration.Security == null || Zen.App.Current.Orchestrator.Person?.HasAnyPermissions(Configuration.Security.ReadPermission) == true;
        protected bool CanWrite => Configuration.Security == null || Zen.App.Current.Orchestrator.Person?.HasAnyPermissions(Configuration.Security.WritePermission) == true;
        protected bool CanRemove => Configuration.Security == null || Zen.App.Current.Orchestrator.Person?.HasAnyPermissions(Configuration.Security.RemovePermission) == true;

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

        private EndpointConfiguration Configuration
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
                        Security = (DataSecurityAttribute)Attribute.GetCustomAttribute(currentType, typeof(DataSecurityAttribute)),
                        Behavior = (DataBehaviorAttribute)Attribute.GetCustomAttribute(currentType, typeof(DataBehaviorAttribute))
                    };

                    _attributeResolutionCache.TryAdd(currentType, newDefinition);

                    return newDefinition;
                }
            }
        }

        [Route("moderation/status"), HttpGet]
        public virtual IActionResult GetStatus() => PrepareResponse(new { count = InternalCount() });

        internal IActionResult PrepareResponse(object content = null, EActionScope scope = EActionScope.Collection, T model = null, HttpStatusCode status = HttpStatusCode.OK)
        {
            var accessControl = new DataAccessControl
            {
                Read = CanRead,
                Write = CanWrite,
                Remove = CanRemove
            };

            Response.Headers?
                .AddModelHeaders(ref accessControl, Request.Query, scope, model)?
                .AddHeaders(accessControl.GetAccessHeaders())?
                .AddMutatorHeaders<T>(RequestMutator);

            var result = new ObjectResult(content) { StatusCode = (int)status };
            return result;
        }

        private static long InternalCount()
        {
            if (Zen.App.Current.Orchestrator?.Person == null) return 0;

            long preRet;
            var hs = ModerationHelper.Setup<T>().ModerationActions;

            if (!hs.Moderate) { preRet = ModerationTask<T>.Count(); }
            else
            {
                var q = new { AuthorLocator = Zen.App.Current.Orchestrator?.Person.Locator }.ToJson();
                preRet = ModerationTask<T>.Query(q).Count();
            }

            return preRet;
        }

        private IEnumerable<ModerationTask<T>> InternalGetAllTasks()
        {
            IEnumerable<ModerationTask<T>> preRet = new List<ModerationTask<T>>();

            if (Zen.App.Current.Orchestrator?.Person == null) return preRet;

            var hs = ModerationHelper.Setup<T>().ModerationActions;

            if (hs.Moderate) { preRet = ModerationTask<T>.All().ToList(); }
            else
            {
                var q = new { AuthorLocator = Zen.App.Current.Orchestrator?.Person.Locator }.ToJson();
                preRet = ModerationTask<T>.Query(q).ToList();
            }

            preRet = preRet.OrderBy(i => i.TimeStamp).Reverse();

            return preRet;
        }

        [Route("moderation/task"), HttpGet]
        public virtual IActionResult GetAllTasks()
        {
            var sw = new Stopwatch();

            try
            {
                sw.Start();
                var preRet = InternalGetAllTasks();
                sw.Stop();

                Log.Add<T>($"GET : GetAllTasks OK ({sw.ElapsedMilliseconds} ms)");
                return PrepareResponse(preRet);
            }
            catch (Exception e)
            {
                sw.Stop();
                Log.Warn<T>($"GET : GetAllTasks ERR ({sw.ElapsedMilliseconds} ms): {e.Message}");
                Log.Add<T>(e);
                return StatusCode(500, e.FancyString());
            }
        }

        [Route("moderation/task/{id}"), HttpGet]
        public virtual IActionResult GetTaskById(string id)
        {
            var sw = new Stopwatch();

            try
            {
                sw.Start();
                var preRet = ModerationTask<T>.Get(id);
                sw.Stop();

                Log.Add<T>($"GetTaskById OK ({sw.ElapsedMilliseconds} ms)");
                return PrepareResponse(preRet);
            }
            catch (Exception e)
            {
                sw.Stop();

                Log.Warn<T>($"GetTaskById ERR ({sw.ElapsedMilliseconds} ms): {e.Message}");
                Log.Add<T>(e);
                return StatusCode(500, e.FancyString());
            }
        }

        [Route("moderation/task/count"), HttpGet]
        public virtual IActionResult GetTaskCount()
        {
            var sw = new Stopwatch();

            try
            {
                sw.Start();
                var preRet = InternalCount();
                sw.Stop();

                Log.Add<T>($"GetTaskCount OK ({sw.ElapsedMilliseconds} ms)");
                return PrepareResponse(preRet);
            }
            catch (Exception e)
            {
                sw.Stop();

                Log.Warn<T>($"GetTaskCount ERR ({sw.ElapsedMilliseconds} ms): {e.Message}");
                Log.Add<T>(e);
                return StatusCode(500, e.FancyString());
            }
        }

        [Route("moderation/task/{tid}/approve"), HttpGet]
        public virtual IActionResult WebApiTaskApprove(string tid)
        {
            var hs = ModerationHelper.Setup<T>().ModerationActions;
            if (!hs.Moderate) return StatusCode(403, "User is not a Moderator");

            var sw = new Stopwatch();

            try
            {
                sw.Start();
                var preRet = ModerationTask<T>.Approve(tid);
                sw.Stop();

                Log.Add<T>($"WebApiTaskApprove [{tid}] Approve OK ({sw.ElapsedMilliseconds} ms)");

                return PrepareResponse(preRet);
            }
            catch (Exception e)
            {
                sw.Stop();
                Log.Warn<T>($"WebApiTaskApprove ERR ({sw.ElapsedMilliseconds} ms): {e.Message}");
                Log.Add<T>(e);
                return StatusCode(500, e.FancyString());
            }
        }

        [Route("moderation/task/{tid}/reject"), HttpGet]
        public virtual IActionResult WebApiTaskReject(string tid)
        {
            var hs = ModerationHelper.Setup<T>().ModerationActions;
            if (!hs.Moderate) return StatusCode(403, "User is not a Moderator");

            var sw = new Stopwatch();

            try
            {
                sw.Start();
                var preRet = ModerationTask<T>.Reject(tid);
                sw.Stop();

                Log.Add<T>($"WebApiTaskReject ID [{tid}] Reject OK ({sw.ElapsedMilliseconds} ms)");

                return PrepareResponse(preRet);
            }
            catch (Exception e)
            {
                sw.Stop();
                Log.Warn<T>($"WebApiTaskReject ERR ({sw.ElapsedMilliseconds} ms): {e.Message}");
                Log.Add<T>(e);
                return StatusCode(500, e.FancyString());
            }
        }

        [Route("moderation/task/{tid}/withdraw"), HttpGet]
        public virtual IActionResult WebApiTaskWithdraw(string tid)
        {
            var sw = new Stopwatch();

            try
            {
                sw.Start();
                var preRet = ModerationTask<T>.Withdraw(tid);
                sw.Stop();

                Log.Add<T>($"WebApiTaskWithdraw ID [{tid}] withdraw OK ({sw.ElapsedMilliseconds} ms)");

                return PrepareResponse(preRet);
            }
            catch (Exception e)
            {
                sw.Stop();
                Log.Warn<T>($"WebApiTaskWithdraw ERR ({sw.ElapsedMilliseconds} ms): {e.Message}");
                Log.Add<T>(e);
                return StatusCode(500, e.FancyString());
            }
        }
    }
}