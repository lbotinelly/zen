using System.Collections.Generic;
using System.Linq;

namespace Zen.Base.Module.Data.Pipeline.SetVersion
{
    public class SetVersion : IBeforeActionPipeline
    {
        #region Implementation of IPipelinePrimitive

        public Dictionary<string, object> Headers<T>() where T : Data<T>
        {
            var mc = Extensions.Configuration<T>();

            if (!mc.CanRead()) return new Dictionary<string, object>();

            var ctx = new List<string>();

            if (mc.CanRead()) ctx.Add("read");
            if (mc.CanModify()) ctx.Add("modify");
            if (mc.CanBrowse()) ctx.Add("browse");

            return new Dictionary<string, object> { { "x-zen-setversion", ctx } };
        }

        #endregion

        #region Implementation of IBeforeActionPipeline

        public T Process<T>(EActionType type, EActionScope scope, Mutator mutator, T current, T source) where T : Data<T> { return current; }

        public KeyValuePair<string, string>? ParseRequest(Dictionary<string, List<string>> requestData)
        {
            if (!requestData.ContainsKey(Mutator.CommonMetadataKeys.Set)) return null;
            return new KeyValuePair<string, string>(Mutator.CommonMetadataKeys.Set, requestData[Mutator.CommonMetadataKeys.Set].FirstOrDefault());
        }

        #endregion
    }
}