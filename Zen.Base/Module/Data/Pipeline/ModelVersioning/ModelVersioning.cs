using System;
using System.Collections.Generic;
using System.Linq;
using Zen.Base.Extension;

namespace Zen.Base.Module.Data.Pipeline.ModelVersioning
{
    public class ModelVersioning : IAfterActionPipeline
    {
        #region Implementation of IPipelinePrimitive

        public Dictionary<string, object> Headers<T>() where T : Data<T> { return null; }

        public void Process<T>(EActionType type, EActionScope scope, Mutator mutator, T current, T source) where T : Data<T>
        {
            if (scope != EActionScope.Model) return;

            var sourceId = current.GetDataKey();

            var logModel = new ModelVersioningContainer<T>
            {
                SourceId = sourceId,
                Entry = current,
                Action = type
            };

            try
            {
                if (type == EActionType.Update)
                {
                    var mfl = new List<string>();
                    string mfls = null;

                    var sj = source.ToJson();
                    var cj = current.ToJson();

                    if (sj == cj) return; // Completely similar records. Ignore.

                    try
                    {
                        var so = source.ToPropertyDictionary();
                        var co = current.ToPropertyDictionary();

                        var afl = new List<string>();
                        afl.AddRange(so.Keys);

                        foreach (var coKey in co.Keys)
                            if (!afl.Contains(coKey))
                                afl.Add(coKey);

                        foreach (var i in afl)
                        {
                            var sv = so.ContainsKey(i) ? so[i] : null;
                            var cv = co.ContainsKey(i) ? co[i] : null;

                            if (!sv.ToJson().Equals(cv.ToJson())) mfl.Add(i);
                        }

                        if (mfl.Count > 0) mfls = mfl.Aggregate((i, j) => i + ", " + j);
                    } catch (Exception e) { }

                    var ss = sj.Length;
                    var cs = cj.Length;
                    var diff = cs - ss;

                    var plural = Math.Abs(diff) > 1 ? "s" : "";

                    if (diff > 0) logModel.Summary = $"{Math.Abs(diff)} character{plural} added";
                    if (diff < 0) logModel.Summary = $"{Math.Abs(diff)} character{plural} removed";
                    if (diff == 0) logModel.Summary = $"Size kept";

                    if (mfls != null) logModel.Summary += ", modified: " + mfls;
                }
            } catch (Exception e){ }


            logModel.Insert();
        }

        #endregion
    }
}