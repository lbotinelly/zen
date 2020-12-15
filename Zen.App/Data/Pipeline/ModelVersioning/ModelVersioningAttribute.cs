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
    public class ModelVersioningAttribute : Attribute, IAfterActionPipeline
    {
        #region Implementation of IPipelinePrimitive

        public string PipelineName { get; set; } = "Model Versioning";
        public Dictionary<string, object> Headers<T>(ref DataAccessControl accessControl, Dictionary<string, StringValues> requestHeaders, EActionScope scope, T model) where T : Data<T> { return null; }

        public void Process<T>(EActionType type, EActionScope scope, Mutator mutator, T current, T source) where T : Data<T>
        {
            if (scope != EActionScope.Model) return;

            var sourceId = current.GetDataKey();

            var versionModel = new ModelVersioning<T>
            {
                SourceId = sourceId,
                Entry = current,
                Action = type
            };

            try
            {
                if (type == EActionType.Update)
                {
                    var diffMap = new List<string>();
                    string diffExpression = null;

                    var serializedSource = source.ToJson();
                    var serializedModel = current.ToJson();

                    if (serializedSource == serializedModel) return; // Completely similar records. Ignore.

                    try
                    {
                        var sourcePropertyDictionary = source.ToMemberDictionary();
                        var modelPropertyDictionary = current.ToMemberDictionary();

                        var compareMap = new List<string>();
                        compareMap.AddRange(sourcePropertyDictionary.Keys);

                        foreach (var coKey in modelPropertyDictionary.Keys)
                            if (!compareMap.Contains(coKey))
                                compareMap.Add(coKey);

                        foreach (var i in compareMap)
                        {
                            var sourceValue = (sourcePropertyDictionary.ContainsKey(i) ? sourcePropertyDictionary[i] : null).ToJson();
                            var modelValue = (modelPropertyDictionary.ContainsKey(i) ? modelPropertyDictionary[i] : null).ToJson();

                            if (!sourceValue.Equals(modelValue)) diffMap.Add(i);
                        }

                        if (diffMap.Count > 0) diffExpression = diffMap.Aggregate((i, j) => i + ", " + j);
                    } catch (Exception) { }

                    var delta = serializedModel.Length - serializedSource.Length;

                    if (delta > 0) versionModel.Summary = $"+{Math.Abs(delta)}";
                    if (delta < 0) versionModel.Summary = $"-{Math.Abs(delta)}";
                    if (delta == 0) versionModel.Summary = "!=";

                    if (diffExpression!= null) versionModel.Summary += " | " + diffExpression;
                }
            } catch (Exception) { }

            Base.Log.Add<T>($"{versionModel.Id} {type} {versionModel.Summary}");

            versionModel.Insert();
        }

        #endregion
    }
}