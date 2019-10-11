using System;
using System.Collections.Generic;
using System.Linq;
using Zen.Base.Module;
using Zen.Base.Module.Data;
using Zen.Base.Module.Data.Pipeline;

namespace Zen.App.Data.Pipeline.SetVersioning
{
    [AttributeUsage(AttributeTargets.Class)]
    public class DataSetVersion : Attribute, IBeforeActionPipeline
    {
        public string Descriptor => "Set Version";
        public string WritePermission { get; set; } = null;
        public string ReadPermission { get; set; } = null;
        public string NotifyPermission { get; set; } = null;
        public bool NotifyChanges { get; set; } = false;

        #region Implementation of IPipelinePrimitive
        public Dictionary<string, object> Headers<T>() where T : Data<T>
        {
            if (!CanBrowse()) return new Dictionary<string, object>();

            var ctx = new List<string>();

            if (CanBrowse()) ctx.Add("browse");
            if (CanModify()) ctx.Add("modify");

            return new Dictionary<string, object> {{"x-zen-setversion", ctx}};
        }
        #endregion

        #region helpers
        public bool CanModify()
        {
            if (WritePermission == null) return false;

            var res = Current.Orchestrator.Person?.HasAnyPermissions(WritePermission);
            return res.HasValue && res.Value;
        }

        public bool CanBrowse()
        {
            if (ReadPermission == null && WritePermission == null) return true;
            var res = Current.Orchestrator.Person?.HasAnyPermissions(ReadPermission ?? WritePermission);
            return res.HasValue && res.Value;
        }

        #endregion

        #region Implementation of IBeforeActionPipeline

        public T Process<T>(EActionType type, EActionScope scope, Mutator mutator, T current, T source) where T : Data<T> => current;

        public KeyValuePair<string, string>? ParseRequest(Dictionary<string, List<string>> requestData)
        {
            if (!requestData.ContainsKey(Mutator.CommonMetadataKeys.Set)) return null;
            return new KeyValuePair<string, string>(Mutator.CommonMetadataKeys.Set, requestData[Mutator.CommonMetadataKeys.Set].FirstOrDefault());
        }

        #endregion
    }
}