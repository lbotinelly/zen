using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Authentication;
using Microsoft.Extensions.Primitives;
using Zen.Base.Module;
using Zen.Base.Module.Data;
using Zen.Base.Module.Data.Pipeline;

namespace Zen.App.Data.Pipeline.SetVersioning
{
    [AttributeUsage(AttributeTargets.Class)]
    public abstract class SetVersioningPrimitiveAttribute : Attribute, IBeforeActionPipeline
    {
        public string WritePermission { get; set; } = null;
        public string ReadPermission { get; set; } = null;
        public string NotifyPermission { get; set; } = null;
        public bool NotifyChanges { get; set; } = false;
        public string PipelineName => "Set Version";

        #region Implementation of IPipelinePrimitive

        public virtual Dictionary<string, object> Headers<T>(ref DataAccessControl accessControl, Dictionary<string, StringValues> requestHeaders, EActionScope scope, T model) where T : Data<T>
        {
            var ctx = new List<string>();

            if (!CanBrowse())
            {
                accessControl.Read = false;
                return new Dictionary<string, object>();
            }

            ctx.Add("browse");

            if (requestHeaders?.ContainsKey(Mutator.CommonMetadataKeys.Set) == true)
            {
                if (!string.IsNullOrEmpty(requestHeaders[Mutator.CommonMetadataKeys.Set]))
                    if (requestHeaders[Mutator.CommonMetadataKeys.Set] != Constants.CURRENT_LIVE_WORKSET_TAG)
                    {
                        accessControl.Write = false;
                        accessControl.Remove = false;
                    }
            }

            if (CanModify()) ctx.Add("modify");
            else
            {
                accessControl.Write = false;
                accessControl.Remove = false;
            }

            return new Dictionary<string, object> { { "x-zen-setversion", ctx } };
        }

        #endregion

        #region helpers

        public bool CanModify() { return Current.Orchestrator.HasAnyPermissions(WritePermission); }

        public bool CanBrowse() { return Current.Orchestrator.HasAnyPermissions(ReadPermission) | Current.Orchestrator.HasAnyPermissions(WritePermission); }

        #endregion

        #region Implementation of IBeforeActionPipeline

        public T Process<T>(EActionType type, EActionScope scope, Mutator mutator, T current, T source) where T : Data<T> { return current; }

        public KeyValuePair<string, string>? ParseRequest<T>(Dictionary<string, List<string>> requestData) where T : Data<T>
        {
            if (!requestData.ContainsKey(Mutator.CommonMetadataKeys.Set)) return null;

            var code = requestData[Mutator.CommonMetadataKeys.Set].FirstOrDefault();

            if (code == Constants.CURRENT_LIVE_WORKSET_TAG) return null;

            var set = SetVersion<T>.GetByCode(code);

            if (!SetVersion<T>.CanBrowse()) throw new AuthenticationException("User is not authorized to browse sets.");

            if (set == null) throw new InvalidFilterCriteriaException($"[{code}]: Invalid code.");

            var suffix = SetVersion<T>.GetByCode(code).SetTag;

            return new KeyValuePair<string, string>(Mutator.CommonMetadataKeys.Set, suffix);
        }

        #endregion
    }
}