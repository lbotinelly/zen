using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson;
using MongoDB.Driver;
using Zen.Base;
using Zen.Base.Extension;
using Zen.Base.Module;

namespace Zen.Module.Data.MongoDB.Factories
{
    internal static class IndexBuilder
    {
        private static readonly ConcurrentDictionary<string, string> DynamicIndexCache = new ConcurrentDictionary<string, string>();

        internal static void TryCreateIndex<T>(this IMongoCollection<BsonDocument> source, BsonDocument sortDocument) where T : Data<T>
        {
            try
            {
                if (sortDocument.ElementCount == 0) return;

                var cacheKey = typeof(T).FullName + "." + sortDocument.ToJson().HashGuid();

                if (DynamicIndexCache.ContainsKey(cacheKey)) return;

                var indexName = sortDocument.Select(i => (i.Value.ToString() == "1" ? "+" : "-") + i.Name.ToLower())
                    .Aggregate("", (current, next) => current + next);

                DynamicIndexCache[cacheKey] = indexName;

                var indexList = source.Indexes.List().ToList().Select(i => i.GetElement("name").Value.AsString);

                //Check if index exists already.
                // https://stackoverflow.com/questions/35019313/checking-if-an-index-exists-in-mongodb
                if (indexList.Contains(indexName)) return;

                var keys = new List<IndexKeysDefinition<BsonDocument>>();

                foreach (var item in sortDocument)
                    keys.Add(item.Value.AsInt32 == 1
                                 ? Builders<BsonDocument>.IndexKeys.Ascending(item.Name)
                                 : Builders<BsonDocument>.IndexKeys.Descending(item.Name));

                var indexOptions = new CreateIndexOptions { Unique = false, Name = indexName, Background = true };
                var model = new CreateIndexModel<BsonDocument>(Builders<BsonDocument>.IndexKeys.Combine(keys), indexOptions);

                source.Indexes.CreateOne(model);
            }
            catch (Exception e) { Current.Log.Warn<T>($"IndexBuilder: {e.Message}"); }
        }
    }
}