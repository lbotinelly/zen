using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Bson.Serialization.Options;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using Newtonsoft.Json.Linq;
using Zen.Base;
using Zen.Base.Extension;
using Zen.Base.Module;
using Zen.Base.Module.Data;
using Zen.Base.Module.Data.Adapter;
using Zen.Base.Module.Data.Connection;
using Zen.Base.Module.Log;
using Zen.Base.Module.Service;
using Zen.Module.Data.MongoDB.Mapping;
using Zen.Module.Data.MongoDB.Serialization;

namespace Zen.Module.Data.MongoDB
{
    public class MongoDbAdapter<T> : DataAdapterPrimitive<T> where T : Data<T>
    {
        private static readonly List<Type> TypeCache = new List<Type>();
        private IMongoClient _client;
        private string _keyMember;
        private Type _refType;
        private Settings<T> _statements;
        private DataConfigAttribute _tabledata;
        public IMongoDatabase Database;

        private string Key
        {
            get
            {
                if (_keyMember != null) return _keyMember;
                _keyMember = _statements.KeyMemberName;
                return _keyMember;
            }
        }

        public override void Setup(Settings<T> statements)
        {
            _statements = statements;

            var statementsConnectionString = _statements.ConnectionString ?? "mongodb://localhost:27017/default";

            _refType = typeof(T);

            // There's probably a better way (I hope) but for now...
            try { BsonSerializer.RegisterSerializer(typeof(DateTime), DateTimeSerializer.LocalInstance); } catch { }

            try { BsonSerializer.RegisterSerializer(typeof(JObject), new JObjectSerializer()); } catch { }

            try { BsonSerializer.RegisterSerializer(typeof(JValue), new JValueSerializer()); } catch { }

            try { BsonSerializer.RegisterSerializer(typeof(JArray), new JArraySerializer()); } catch { }

            try { BsonTypeMapper.RegisterCustomTypeMapper(typeof(JObject), new JObjectMapper()); } catch { }

            _client = Instances.GetClient(statementsConnectionString);
            var server = _client.Settings.Servers.FirstOrDefault()?.Host;

            var storageName = MongoUrl.Create(statementsConnectionString).DatabaseName;

            if (SourceBundle is IStorageContainerResolver) storageName = ((IStorageContainerResolver) SourceBundle).GetStorageContainerName(_statements?.EnvironmentCode);

            if (string.IsNullOrEmpty(storageName)) storageName = "storage";

            // https://jira.mongodb.org/browse/CSHARP-965
            // http://stackoverflow.com/questions/19521626/mongodb-convention-packs

            ConventionRegistry.Register("EnumStringConvention", new ConventionPack {new EnumRepresentationConvention(BsonType.String)}, t => true);
            ConventionRegistry.Register("ignore extra elements", new ConventionPack {new IgnoreExtraElementsConvention(true)}, t => true);
            ConventionRegistry.Register("DictionaryRepresentationConvention", new ConventionPack {new DictionaryRepresentationConvention(DictionaryRepresentation.ArrayOfArrays)}, _ => true);
            ConventionRegistry.Register("EnumStringConvention", new ConventionPack {new EnumRepresentationConvention(BsonType.String)}, t => true);

            Database = _client.GetDatabase(storageName);

            // Current.Log.Add($"{typeof(T).FullName} {Database.Client?.Settings?.Credential?.Username}:{Database?.DatabaseNamespace}@{server} - REGISTERING", Message.EContentType.StartupSequence);

            _statements = Info<T>.Settings;
            _tabledata = Info<T>.Configuration;

            if (_tabledata.IgnoreEnvironmentPrefix != true) _collectionPrefix = $"{_statements.EnvironmentCode}.";

            RegisterGenericChain(typeof(T));

            SetBaseCollectionName();
        }

        public override void Initialize()
        {
            // Not really necessary.
        }

        #region Adapter-specific

        private void SetBaseCollectionName()
        {
            _collectionNamespace = _tabledata?.SetPrefix ?? _statements.TypeNamespace;
            var typeName = _tabledata?.SetName ?? _statements.TypeName;

            if (typeof(IStorageCollectionResolver).GetTypeInfo().IsAssignableFrom(_refType.GetTypeInfo())) typeName = ((IStorageCollectionResolver) _refType.CreateInstance()).GetStorageCollectionName();

            _collectionName = typeName;

            ReferenceCollectionName = GetCollectionName();
        }

        private string GetCollectionName(string suffix = "")
        {
            var parsedSuffix = !string.IsNullOrEmpty(suffix) ? _collectionPrefixSeparator + suffix : "";
            return $"{_collectionPrefix}{_collectionNamespace}.{_collectionName}{parsedSuffix}";
        }

        private string _collectionPrefix = "";
        private readonly string _collectionPrefixSeparator = "#";
        private string _collectionNamespace = "";
        private string _collectionName = "";

        private void RegisterBsonType(Type type, bool mapId = true)
        {
            if (BsonClassMap.IsClassMapRegistered(type)) return;

            Current.Log.KeyValuePair("MongoDbAdapter ClassMap", type.FullName);

            var classMapDefinition = typeof(BsonClassMap<>);
            var classMapType = classMapDefinition.MakeGenericType(type);
            var classMap = (BsonClassMap) Activator.CreateInstance(classMapType);

            try
            {
                classMap.AutoMap();
                if (mapId) classMap.MapIdProperty(Key);

                BsonClassMap.RegisterClassMap(classMap);
            } catch (Exception e) { Log.KeyValuePair(type.FullName, e.Message, Message.EContentType.Warning); }
        }

        private void RegisterGenericChain(Type type)
        {
            try
            {
                while (type != null && type.BaseType != null)
                {
                    if (!TypeCache.Contains(type))
                    {
                        TypeCache.Add(type);

                        if (!type.IsAbstract)
                            if (!type.IsGenericType) RegisterBsonType(type);
                            else
                                foreach (var t in type.GetTypeInfo().GenericTypeArguments)
                                    RegisterGenericChain(t);
                    }

                    type = type.BaseType;
                }
            } catch (Exception e) { Current.Log.Add(e, "Error registering class " + type?.Name + ": " + e.Message); }
        }

        private void ClassMapInitializer(BsonClassMap<MongoDbAdapter<T>> cm)
        {
            cm.AutoMap();
            cm.MapIdMember(c => c.Key);
        }

        #endregion

        #region Interceptor calls

        public override T Get(string key, Mutator mutator = null)
        {
            try
            {
                var filter = Builders<BsonDocument>.Filter.Eq("_id", key);
                var collection = Collection(mutator).Find(filter).ToList();

                var model = collection.FirstOrDefault();

                if (model == null)
                {
                    var isNumeric = long.TryParse(key, out var n);

                    if (isNumeric)
                    {
                        filter = Builders<BsonDocument>.Filter.Eq("_id", n);
                        collection = Collection(mutator).Find(filter).ToList();
                        model = collection.FirstOrDefault();
                    }
                }

                return model == null ? null : BsonSerializer.Deserialize<T>(model);
            } catch (FormatException e)
            {
                if (TryRegisterByException(e)) return Get(key, mutator);
                throw;
            } catch (Exception e)
            {
                Current.Log.Add($"{Database.Client.Settings.Credential.Username}@{Database.DatabaseNamespace} - {Collection(mutator).CollectionNamespace}:{key} {e.Message}", Message.EContentType.Warning);
                throw;
            }
        }

        public override IEnumerable<T> Get(IEnumerable<string> keys, Mutator mutator = null)
        {
            var filter = Builders<BsonDocument>.Filter.In("_id", keys);
            var col = Collection(mutator).Find(filter).ToList();
            var res = col.Select(i => BsonSerializer.Deserialize<T>(i)).ToList();
            return res;
        }

        public override IEnumerable<T> Query(string statement) => Query<T>(statement.ToModifier());

        public override IEnumerable<T> Query(Mutator mutator = null) => Query<T>(mutator);

        public override IEnumerable<T> Where(Expression<Func<T, bool>> predicate, Mutator mutator = null)
        {
            try { return Collection<T>(mutator).AsQueryable().Where(predicate).ToList(); } catch (FormatException e)
            {
                if (TryRegisterByException(e)) return Where(predicate, mutator);

                throw;
            }
        }

        private bool TryRegisterByException(FormatException formatException)
        {
            var parts = formatException.Message.Split("'").ToList();

            if (parts.Count != 3) return false;

            try
            {
                var probe = IoC.TypeByName(parts[1])?.FirstOrDefault();
                RegisterBsonType(probe, false);

                return true;
            } catch (Exception e) { }

            return false;
        }

        public override IEnumerable<TU> Query<TU>(string statement) => Query<TU>(statement.ToModifier());

        public override IEnumerable<TU> Query<TU>(Mutator mutator = null)
        {
            try
            {
                var fluentCollection = Collection(mutator).ApplyTransform<T>(mutator?.Transform);

                var colRes = fluentCollection.ToListAsync();
                Task.WhenAll(colRes);

                var res = colRes.Result.AsParallel().Select(v => BsonSerializer.Deserialize<TU>(v)).ToList();
                return res;
            } catch (FormatException e)
            {
                if (TryRegisterByException(e)) return Query<TU>(mutator);
                throw;
            }
        }

        public override long Count(Mutator mutator = null) => Collection(mutator).CountDocuments(mutator?.Transform?.ToBsonQuery() ?? new BsonDocument());

        public override bool KeyExists(string key, Mutator mutator = null) => Get(key, mutator) != null;

        public override T Insert(T model, Mutator mutator = null) => Upsert(model, mutator);

        public override T Save(T model, Mutator mutator = null) => Upsert(model, mutator);

        public override T Upsert(T obj, Mutator mutator = null)
        {
            string id = null;
            var isNew = false;

            var targetCollection = Collection(mutator);

            try
            {
                id = obj.GetDataKey();

                if (string.IsNullOrEmpty(id))
                {
                    id = GetNewKey();
                    obj.SetDataKey(id);

                    isNew = true;
                }

                lock (StaticLock<T>.Lock)
                {
                    var probe = isNew ? null : Get(id, mutator);
                    var document = BsonSerializer.Deserialize<BsonDocument>(obj.ToJson());
                    if (probe == null) { targetCollection.InsertOne(document); }
                    else
                    {
                        var filter = Builders<BsonDocument>.Filter.Eq("_id", id);
                        targetCollection.ReplaceOne(filter, document);
                    }
                }

                return obj;
            } catch (Exception e)
            {
                Current.Log.Warn<T>($"{Database.Client.Settings?.Credential?.Username}@{Database?.DatabaseNamespace} - {Collection(mutator).CollectionNamespace}:{id} {e.Message}");
                Current.Log.Warn<T>($"{obj.ToJson()}");
                throw;
            }
        }

        private readonly Dictionary<string, IMongoCollection<BsonDocument>> _collectionCache = new Dictionary<string, IMongoCollection<BsonDocument>>();

        private IMongoCollection<BsonDocument> Collection(Mutator mutator = null)
        {
            var referenceCode = mutator?.SetCode ?? "";

            lock (_collectionCache)
            {
                if (_collectionCache.ContainsKey(referenceCode)) return _collectionCache[referenceCode];

                _collectionCache[referenceCode] = Database.GetCollection<BsonDocument>(GetCollectionName(referenceCode));

                try
                {
                    var indexOptions = new CreateIndexOptions {Unique = false, Name = "fullTextSearch", Background = true};
                    var model = new CreateIndexModel<BsonDocument>(Builders<BsonDocument>.IndexKeys.Text("$**"), indexOptions);
                    _collectionCache[referenceCode].Indexes.CreateOne(model);
                } catch (Exception e) { Current.Log.Info<T>(e.Message); }

                return _collectionCache[referenceCode];
            }
        }

        private IMongoCollection<T> Collection<T>(Mutator mutator = null)
        {
            var referenceCode = mutator?.SetCode;
            var collectionName = GetCollectionName(referenceCode);
            var collection = Database.GetCollection<T>(collectionName);

            return collection;
        }

        public override IEnumerable<T> BulkInsert(IEnumerable<T> models, Mutator mutator = null) => BulkUpsert(models, mutator);

        public override IEnumerable<T> BulkSave(IEnumerable<T> models, Mutator mutator = null) => BulkUpsert(models, mutator);

        public override IEnumerable<T> BulkUpsert(IEnumerable<T> models, Mutator mutator = null)
        {
            var c = 0;

            if (models == null) return null;

            var enumerable = models.ToList();

            if (!enumerable.Any()) return null;

            T current = null;

            try
            {
                var replacementBuffer = new List<ReplaceOneModel<BsonDocument>>();
                var returnModelBuffer = new List<T>();

                foreach (var i in enumerable)
                {
                    c++;

                    current = i;

                    if (string.IsNullOrEmpty(i.GetDataKey())) i.SetDataKey(GetNewKey());

                    replacementBuffer.Add(
                        new ReplaceOneModel<BsonDocument>(
                                Builders<BsonDocument>.Filter.Eq("_id", i.GetDataKey()),
                                BsonSerializer.Deserialize<BsonDocument>(i.ToJson()))
                            {IsUpsert = true});
                    returnModelBuffer.Add(i);
                }

                Collection(mutator).BulkWrite(replacementBuffer.ToArray());

                return returnModelBuffer;
            } catch (Exception e)
            {
                Current.Log.Warn<T>($"{Collection(mutator).CollectionNamespace}:BulkSave {c}/{enumerable.Count} items: {e.Message}");
                Current.Log.Warn<T>($"{current.ToJson()}");
                Current.Log.Add<T>(e);
                throw;
            }
        }

        public override void BulkRemove(IEnumerable<string> keys, Mutator mutator = null)
        {
            var filter = Builders<BsonDocument>.Filter.In("_id", keys);
            Collection(mutator).DeleteMany(filter);
        }

        public override void BulkRemove(IEnumerable<T> models, Mutator mutator = null) => BulkRemove(models.Select(i => i.GetDataKey()), mutator);

        public override void Remove(string locator, Mutator mutator = null)
        {
            var filter = Builders<BsonDocument>.Filter.Eq("_id", locator);
            Collection(mutator).DeleteOne(filter);
        }

        public override void Remove(T model, Mutator mutator = null) => Remove(model.GetDataKey(), mutator);

        public override void RemoveAll(Mutator mutator = null) => Collection(mutator).DeleteMany(FilterDefinition<BsonDocument>.Empty);

        public override void DropSet(string setName) => Database.DropCollection(GetCollectionName(setName));

        public override void CopySet(string sourceSetIdentifier, string targetSetIdentifier, bool flushDestination = false)
        {
            if (flushDestination) DropSet(targetSetIdentifier);

            var sourceName = GetCollectionName(sourceSetIdentifier);
            var targetName = GetCollectionName(targetSetIdentifier);

            var aggDoc = new Dictionary<string, object>
            {
                {"aggregate", sourceName},
                {
                    "pipeline", new[]
                    {
                        new Dictionary<string, object> {{"$match", new BsonDocument()}},
                        new Dictionary<string, object> {{"$out", targetName}}
                    }
                },
                {"cursor", new Dictionary<string, object> {{"batchSize", 8192}}}
            };

            var doc = new BsonDocument(aggDoc);
            var command = new BsonDocumentCommand<BsonDocument>(doc);

            Database.RunCommand(command);
        }

        #endregion
    }
}