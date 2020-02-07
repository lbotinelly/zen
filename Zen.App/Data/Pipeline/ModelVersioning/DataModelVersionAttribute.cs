using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Primitives;
using Zen.Base.Extension;
using Zen.Base.Module;
using Zen.Base.Module.Data;
using Zen.Base.Module.Data.Pipeline;

namespace Zen.App.Data.Pipeline.ModelVersioning
{
    [AttributeUsage(AttributeTargets.Class)]
    public class DataModelVersionAttribute : Attribute, IAfterActionPipeline
    {
        #region Implementation of IPipelinePrimitive

        public string PipelineName { get; set; } = "Model Versioning";
        public Dictionary<string, object> Headers<T>(ref DataAccessControl accessControl, Dictionary<string, StringValues> requestHeaders, EActionScope scope, T model) where T : Data<T> { return null; }

        public void Process<T>(EActionType type, EActionScope scope, Mutator mutator, T current, T source) where T : Data<T>
        {
            if (scope != EActionScope.Model) return;

            var sourceId = current.GetDataKey();

            var versionModel = new ModelVersion<T>
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
                    } catch (Exception) { }

                    var ss = sj.Length;
                    var cs = cj.Length;
                    var diff = cs - ss;

                    var plural = Math.Abs(diff) > 1 ? "s" : "";

                    if (diff > 0) versionModel.Summary = $"{Math.Abs(diff)} character{plural} added";
                    if (diff < 0) versionModel.Summary = $"{Math.Abs(diff)} character{plural} removed";
                    if (diff == 0) versionModel.Summary = $"Size kept";

                    if (mfls != null) versionModel.Summary += ", modified: " + mfls;
                }
            } catch (Exception) { }

            versionModel.Insert();
        }

        #endregion
    }
}