using System;
using System.Collections.Generic;
using System.Linq;
using Zen.Base.Extension;
using Zen.Base.Module;

namespace Zen.Pebble.CrossModelMap.Change
{
    public class ChangeTracker<T, TU> where TU : Data<TU>
    {
        public MultiSet DataSets { get; } = new MultiSet();

        public Func<T, string> IdentifierFunc { get; private set; }

        public Func<T, string> ChecksumFunc { get; } = arg => arg.ToJson().Sha512Hash();

        public Func<T, string, string> SourceValueHandlerFunc { get; set; }

        public ChangeTrackerConfiguration Configuration { get; set; } = new ChangeTrackerConfiguration();

        public void ClearChangeTrack()
        {
            ChangeEntry<T>.RemoveAll();
        }

        public Dictionary<string, ChangeEntry<T>> Changes { get; private set; }

        public Func<T, TU> TransformAction { get; set; }
        private Action<(T sourceData, TU targetModel)> ComplexTransformAction { get; set; }


        public Action<(string HandlerType, string Source, object Current, ConvertToModelTypeResult Result)>
            ConvertToModelTypeAction
        { get; set; }

        public ChangeTracker<T, TU> ComplexTransform(Action<(T sourceData, TU targetModel)> function)
        {
            ComplexTransformAction = function;
            return this;
        }

        public ChangeTracker<T, TU> ConvertToModelType(
            Action<(string HandlerType, string Source, object Current, ConvertToModelTypeResult Result)> action)
        {
            ConvertToModelTypeAction = action;
            return this;
        }

        public ChangeTracker<T, TU> SourceDataByPath(Func<T, string, string> function)
        {
            SourceValueHandlerFunc = function;
            return this;
        }

        public ChangeTracker<T, TU> Transform(Func<T, TU> function)
        {
            TransformAction = function;
            return this;
        }

        public ChangeTracker<T, TU> Identifier(Func<T, string> function)
        {
            IdentifierFunc = function;
            return this;
        }

        public string GetIdentifier(T model)
        {
            return IdentifierFunc(model);
        }

        public void Run()
        {
            Start();

            ProcessChanges();

            Changes.Save();
            DataSets.Save();
        }

        public virtual void Start()
        {
        }

        public Dictionary<string, ChangeEntry<T>> FetchChanges(IEnumerable<T> modelCollection)
        {
            var sourceSetMap = modelCollection.ToList().ToDictionary(GetIdentifier, i => i);

            var outputSet = new Dictionary<string, ChangeEntry<T>>();

            // First get all Identifiers.
            var modelIdentifiers = sourceSetMap.Keys.ToList();

            var modelEntries = ChangeEntry<T>.GetMap(modelIdentifiers);

            var currentEntries = modelEntries.Where(i => i.Value != null).ToDictionary(i => i.Key, i => i.Value);
            var modelEntriesKeys = currentEntries.Keys.ToList();


            foreach (var recordedChange in currentEntries.Where(recordedChange =>
                recordedChange.Value.Checksum != ChecksumFunc(sourceSetMap[recordedChange.Key])))
            {
                outputSet[recordedChange.Key] = recordedChange.Value;
                outputSet[recordedChange.Key].Type = ChangeEntry<T>.EType.Update;
            }


            var newEntries = modelIdentifiers.Except(modelEntriesKeys).ToList();

            foreach (var newEntry in newEntries)
            {
                var targetModel = sourceSetMap[newEntry];

                outputSet[newEntry] = new ChangeEntry<T>
                {
                    Checksum = ChecksumFunc(targetModel),
                    Model = targetModel,
                    Type = ChangeEntry<T>.EType.New,
                    Id = newEntry
                };
            }

            Changes = outputSet;

            return outputSet;
        }

        public ChangeTracker<T, TU> Configure(Action<ChangeTrackerConfiguration> action)
        {
            action.Invoke(Configuration);
            return this;
        }

        public void ProcessChanges()
        {
            ProcessChanges(Changes);
        }

        public void ProcessChanges(Dictionary<string, ChangeEntry<T>> changes)
        {
            foreach (var value in changes.Values)
                try
                {
                    // First resolve the target record.
                    var targetModel = TransformAction(value.Model);

                    // First use the simple Map iteration.
                    ApplyMappedData(targetModel, value);

                    //If defined, use the complex transformation function
                    ComplexTransformAction?.Invoke((value.Model, targetModel ));

                    value.Result = ChangeEntry<T>.EResult.Success;
                }
                catch (Exception e)
                {
                    value.Result = ChangeEntry<T>.EResult.Fail;
                    value.ResultMessage = e.Message;
                }
        }


        private void ApplyMappedData(TU model, ChangeEntry<T> value)
        {
            foreach (var (key, definition) in Configuration.MemberMapping.KeyMaps)
            {
                var sourceValue = SourceValueHandlerFunc(value.Model, key);

                if (string.IsNullOrEmpty(sourceValue) && definition.AternateSources?.Count > 0)
                {
                    foreach (var altSource in definition.AternateSources)
                    {
                        sourceValue = SourceValueHandlerFunc(value.Model, altSource);
                        if (sourceValue != null) break;
                    }
                }

                 
                var destinationValue = model.GetMemberValue(definition.Target);

                var result = new ConvertToModelTypeResult();

                ConvertToModelTypeAction.Invoke((definition.Handler, sourceValue, destinationValue, result));

                if (result.Success) model.SetMemberValue(definition.Target, result.Value);
            }
        }

        public class ConvertToModelTypeResult
        {
            public bool Success = false;
            public object Value { get; set; }
        }

        public class ChangeTrackerConfiguration
        {
            public TimeSpan? StaleRecordTimespan { get; set; } = TimeSpan.MaxValue;
            public string Set { get; set; }
            public string Collection { get; set; }
            public MapDefinition MemberMapping { get; set; } = null;
            public string SourceIdentifierPath { get; set; }
        }
    }
}