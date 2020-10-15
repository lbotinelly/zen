using System;
using System.Collections.Generic;
using System.Linq;
using Zen.Base.Extension;

namespace Zen.Pebble.CrossModelMap.Change
{
    public class ChangeTracker<T>
    {
        public Func<T, string> IdentifierMethod { get; private set; }

        public Func<T, string> ChecksumFunction { get; } = arg => arg.ToJson().Sha512Hash();

        public ChangeTrackerConfiguration Configuration { get; set; } = new ChangeTrackerConfiguration();

        public ChangeTracker<T> Identifier(Func<T, string> function)
        {
            IdentifierMethod = function;
            return this;
        }

        public string GetIdentifier(T model)
        {
            return IdentifierMethod(model);
        }

        public Dictionary<string, ChangeEntry<T>> DetectChanges(IEnumerable<T> modelCollection)
        {
            var sourceSetMap = modelCollection.ToList().ToDictionary(GetIdentifier, i => i);

            var outputSet = new Dictionary<string, ChangeEntry<T>>();

            // First get all Identifiers.
            var modelIdentifiers = sourceSetMap.Keys.ToList();

            var modelEntries = ChangeEntry<T>.GetMap(modelIdentifiers);

            var currentEntries = modelEntries.Where(i => i.Value != null).ToDictionary(i => i.Key, i => i.Value);
            var modelEntriesKeys = currentEntries.Keys.ToList();


            foreach (var recordedChange in currentEntries.Where(recordedChange =>
                recordedChange.Value.Checksum != ChecksumFunction(sourceSetMap[recordedChange.Key])))
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
                    Checksum = ChecksumFunction(targetModel),
                    Model = targetModel,
                    Type = ChangeEntry<T>.EType.New,
                    Id = newEntry
                };
            }


            return outputSet;
        }

        public ChangeTracker<T> Configure(Action<ChangeTrackerConfiguration> action)
        {
            action.Invoke(Configuration);
            return this;
        }

        public class ChangeTrackerConfiguration
        {
            public TimeSpan? StaleRecordTimespan { get; set; } = TimeSpan.MaxValue;
            public string Set { get; set; }
            public string Collection { get; set; }
        }
    }
}