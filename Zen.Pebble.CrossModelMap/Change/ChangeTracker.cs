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

        public Dictionary<string, ChangeEntry<T>> Changes { get; private set; }

        public Func<T, TU> ResolveTargetModelFunc { get; set; } = arg => null;

        public ChangeTracker<T, TU> ContentTypeHandler(Action<string, string, object, ContentTypeHandlerResult> action)
        {
            ContentTypeHandlerAction = action;
            return this;
        }

        public Action<string, string, object, ContentTypeHandlerResult> ContentTypeHandlerAction { get; set; }
        
        public ChangeTracker<T, TU> SourceValueHandler(Func<T, string, string> function)
        {
            SourceValueHandlerFunc = function;
            return this;
        }

        public ChangeTracker<T, TU> ResolveTargetModel(Func<T, TU> function)
        {
            ResolveTargetModelFunc = function;
            return this;
        }

        public ChangeTracker<T, TU> Identifier(Func<T, string> function)
        {
            IdentifierFunc = function;
            return this;
        }

        public virtual void Process()
        {
        }


        public string GetIdentifier(T model)
        {
            return IdentifierFunc(model);
        }

        public Dictionary<string, ChangeEntry<T>> GetChanges(IEnumerable<T> modelCollection)
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

        public void ProcessChanges(Func<ChangeTracker<T, TU>, T, TU> function)
        {
            ProcessChanges(Changes, function);
        }

        public void ProcessChanges(Dictionary<string, ChangeEntry<T>> changes,
            Func<ChangeTracker<T, TU>, T, TU> function)
        {
            foreach (var (key, value) in changes)
                try
                {
                    // First resolve the target record.
                    var model = ResolveTargetModelFunc(value.Model);

                    // First use the simple Map iteration.

                    ApplyValuesByMap(model, key, value);


                    //Then, if present, use the function.
                    if (function != null)
                    {
                        var result = function(this, value.Model);
                    }

                    value.Result = ChangeEntry<T>.EResult.Success;
                }
                catch (Exception e)
                {
                    value.Result = ChangeEntry<T>.EResult.Fail;
                    value.ResultMessage = e.Message;
                }
        }


        private TU ApplyValuesByMap(TU model, string key, ChangeEntry<T> value)
        {
            foreach (var (k, v) in Configuration.MemberMapping.KeyMaps)
            {
                var sourceValue = SourceValueHandlerFunc(value.Model, k);
                var destinationValue = model.GetMemberValue(v.Target);

                var result = new ContentTypeHandlerResult();

                ContentTypeHandlerAction.Invoke(v.Handler, sourceValue, destinationValue, result);

                if (result.Success) model.SetMemberValue(v.Target, result.Value);
            }

            return model;
        }

        public class ContentTypeHandlerResult
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