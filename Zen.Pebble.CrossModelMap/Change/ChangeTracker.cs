using System;
using System.Collections.Generic;
using System.Linq;
using Zen.Base;
using Zen.Base.Extension;
using Zen.Base.Module;
using Zen.Base.Module.Log;

namespace Zen.Pebble.CrossModelMap.Change
{
    public class ChangeTracker<T, TU> where TU : Data<TU>
    {
        public MultiSet DataSets { get; } = new MultiSet();

        public ScopedTimeLog ScopedLog = new ScopedTimeLog();

        public Func<T, string> IdentifierFunc { get; private set; }

        public Func<T, string> ChecksumFunc { get; } = arg => arg.ToJson().Sha512Hash();

        public Func<T, string, string> SourceValueFunc { get; set; }

        public ChangeTrackerConfiguration Configuration { get; set; } = new ChangeTrackerConfiguration();

        public Dictionary<string, ChangeEntry<T>> Changes { get; private set; }

        public Func<T, TU> ResolveReferenceAction { get; set; }
        private Action<(T Model, TU targetModel, ScopedTimeLog timeLog)> ComplexTransformAction { get; set; }


        public Action<(string HandlerType, string Source, object Current, ConvertToModelTypeResult Result)>
            ConvertToModelTypeAction
        { get; set; }

        public Action OnCommitAction { get; set; }

        public ChangeTracker<T, TU> Prepare(Action action)
        {
            this.PrepareAction = action;
            return this;
        }

        public Action PrepareAction { get; set; }


        public Action<ChangeBag> SourceModelAction { get; set; }

        public void ClearChangeTrack()
        {
            try
            {
                ChangeEntry<T>.RemoveAll();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public ChangeTracker<T, TU> ComplexTransform(Action<(T sourceData, TU targetModel, ScopedTimeLog timeLog)> function)
        {
            ComplexTransformAction = function;
            return this;
        }

        public ChangeTracker<T, TU> OnCommit(Action action)
        {
            OnCommitAction = action;
            return this;
        }

        public ChangeTracker<T, TU> ConvertToModelType(
            Action<(string HandlerType, string Source, object Current, ConvertToModelTypeResult Result)> action)
        {
            ConvertToModelTypeAction = action;
            return this;
        }

        public ChangeTracker<T, TU> SourceValue(Func<T, string, string> function)
        {
            SourceValueFunc = function;
            return this;
        }

        public ChangeTracker<T, TU> ResolveReference(Func<T, TU> function)
        {
            ResolveReferenceAction = function;
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
            var tn = GetType().Name;

            ScopedLog.Start($"{tn} - Starting");

            var changeBag = new ChangeBag { Items = new List<T>() };

            PrepareAction?.Invoke();

            SourceModelAction(changeBag);

            FetchChanges(changeBag.Items);

            if (Changes == null || Changes?.Count == 0)
            {
                ScopedLog.Log($"{tn} - No changes in queue");
            }
            else
            {
                ScopedLog.Log($"{tn} - Process Changes");
                ProcessChanges();

                ScopedLog.Log($"{tn} - Commit Changes");
                Changes.Save();

                ScopedLog.Log($"{tn} - Commit Datasets");
                DataSets.Save();

                if (OnCommitAction != null)
                {
                    ScopedLog.Start($"{tn} - Running post-commit actions");
                    OnCommitAction?.Invoke();
                }
            }

            ScopedLog.Start($"{tn} - Finished");

            ScopedLog.End();
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

        public ChangeTracker<T, TU> SourceModel(Action<ChangeBag> action)
        {
            SourceModelAction = action;
            return this;
        }

        public void ProcessChanges()
        {
            ProcessChanges(Changes);
        }

        public void ProcessChanges(Dictionary<string, ChangeEntry<T>> changes)
        {
            if (changes == null)
            {
                Log.Warn<T>("No changes queued");
                return;
            }

            Log.Add<T>($"Processing {changes.Count} changes");

            foreach (var value in changes.Values)
            {
                var scopedTimeLog = new ScopedTimeLog();

                try
                {

                    // First resolve the target record.
                    scopedTimeLog.Start("Reference resolution");
                    var targetModel = ResolveReferenceAction(value.Model);

                    // First use the simple Map iteration.
                    scopedTimeLog.Start("Data mapping");
                    ApplyMappedData(targetModel, value);

                    //If defined, use the complex transformation function
                    if (ComplexTransformAction != null)
                    {
                        scopedTimeLog.Start("Complex transformation");
                        ComplexTransformAction?.Invoke((value.Model, targetModel, scopedTimeLog));
                    }

                    value.Result = ChangeEntry<T>.EResult.Success;
                }
                catch (Exception e)
                {
                    ScopedLog.Log($"WARN {value.Id} - {scopedTimeLog.LastMessage()}: {e.FlatExceptionMessage()}");
                    value.Result = ChangeEntry<T>.EResult.Fail;
                    value.ResultMessage = e.Message;
                }
            }
        }


        private void ApplyMappedData(TU model, ChangeEntry<T> value)
        {
            if (!(Configuration.MemberMapping?.KeyMaps?.Count > 0)) return;

            foreach (var (key, definition) in Configuration.MemberMapping.KeyMaps)
            {
                var sourceValue = SourceValueFunc(value.Model, key);

                if (string.IsNullOrEmpty(sourceValue) && definition.AternateSources?.Count > 0)
                    foreach (var altSource in definition.AternateSources)
                    {
                        sourceValue = SourceValueFunc(value.Model, altSource);
                        if (sourceValue != null) break;
                    }

                var destinationValue = model.GetMemberValue(definition.Target);

                var result = new ConvertToModelTypeResult();

                ConvertToModelTypeAction.Invoke((definition.Handler, sourceValue, destinationValue, result));

                if (result.Success) model.SetMemberValue(definition.Target, result.Value);
            }
        }

        public class ChangeBag
        {
            public List<T> Items = new List<T>();
            public object Source;
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
            public string CollectionFullIdentifier { get; set; }
        }

    }
}