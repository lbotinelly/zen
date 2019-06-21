using System;
using System.Collections.Generic;
using System.Linq;
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
using Zen.Base.Module;
using Zen.Base.Module.Data;
using Zen.Base.Module.Data.Adapter;
using Zen.Base.Module.Data.Connection;
using Zen.Base.Module.Log;
using Zen.Module.Data.MongoDB.Mapping;
using Zen.Module.Data.MongoDB.Serialization;

namespace Zen.Module.Data.MongoDB
{
    public class MongoDbAdapter : DataAdapterPrimitive
    {
        private static readonly List<Type> TypeCache = new List<Type>();
        private IMongoClient _client;
        private object _instance;

        private string _keyMember;
        private Type _refType;
        private Settings _statements;
        private DataConfigAttribute _tabledata;
        public IMongoCollection<BsonDocument> Collection { get; set; }
        public IMongoDatabase Database;
        public string ReferenceCollectionName;

        private string Key
        {
            get
            {
                if (_keyMember != null) return _keyMember;
                _keyMember = _statements.KeyMemberName;
                return _keyMember;
            }
        }

        public override void Initialize<T>()
        {
            // Check for the presence of text indexes '$**'
            try
            {
                var indexOptions = new CreateIndexOptions { Unique = false, Name = "fullTextSearch" };
                var model = new CreateIndexModel<BsonDocument>(Builders<BsonDocument>.IndexKeys.Text("$**"), indexOptions);
                Collection.Indexes.CreateOne(model);
            }
            catch (Exception e)
            {
                var server = Database.Client?.Settings?.Server?.Host;
                try { server = Database?.Client?.Settings?.Servers.FirstOrDefault()?.Host; } catch (Exception) { }

                Current.Log.Warn<T>($"{Database.Client?.Settings?.Credential?.Username}:{Database?.DatabaseNamespace}@{server} - {Collection?.CollectionNamespace} {e.Message} | ERR Creating index {ReferenceCollectionName}: {e.Message}");
            }
        }

        public override void Setup<T>(Settings statements)
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

            Current.Log.Debug<T>($"Client for {statementsConnectionString}: {_client?.Settings?.Credential?.Username}@{server}");

            var dbname = MongoUrl.Create(statementsConnectionString).DatabaseName;

            if (SourceBundle is IStorageContainerResolver) dbname = ((IStorageContainerResolver)SourceBundle).GetStorageContainerName(_statements?.EnvironmentCode);

            if (string.IsNullOrEmpty(dbname)) dbname = "storage";

            // https://jira.mongodb.org/browse/CSHARP-965
            // http://stackoverflow.com/questions/19521626/mongodb-convention-packs

            var pack = new ConventionPack { new IgnoreExtraElementsConvention(true) };

            ConventionRegistry.Register("ignore extra elements", pack, t => true);
            ConventionRegistry.Register("DictionaryRepresentationConvention", new ConventionPack { new DictionaryRepresentationConvention(DictionaryRepresentation.ArrayOfArrays) }, _ => true);
            ConventionRegistry.Register("EnumStringConvention", new ConventionPack { new EnumRepresentationConvention(BsonType.String) }, t => true);

            Database = _client.GetDatabase(dbname);

            Current.Log.Add($"{typeof(T).FullName} {Database.Client?.Settings?.Credential?.Username}:{Database?.DatabaseNamespace}@{server} - REGISTERING", Message.EContentType.StartupSequence);

            _statements = Data<T>.Info<T>.Settings;
            _tabledata = Data<T>.Info<T>.Configuration;

            RegisterGenericChain(typeof(T));

            SetBaseCollectionName();
        }

        #region Adapter-specific

        private void SetBaseCollectionName()
        {

            var typeName = _refType.FullName;

            if (!string.IsNullOrEmpty(_tabledata?.TablePrefix)) typeName = $"{_tabledata.TablePrefix}.{_refType.Name}";
            if (!string.IsNullOrEmpty(_tabledata?.TableName)) typeName = _tabledata.TableName;

            ReferenceCollectionName = _tabledata?.IgnoreEnvironmentPrefix == true ? typeName : $"{_statements.EnvironmentCode}.{typeName}";
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
                            if (!type.IsGenericType)
                            {
                                if (!BsonClassMap.IsClassMapRegistered(type))
                                {
                                    Current.Log.Add("MongoDbinterceptor: Registering " + type.FullName);

                                    var classMapDefinition = typeof(BsonClassMap<>);
                                    var classMapType = classMapDefinition.MakeGenericType(type);
                                    var classMap = (BsonClassMap)Activator.CreateInstance(classMapType);

                                    // Do custom initialization here, e.g. classMap.SetDiscriminator, AutoMap etc

                                    classMap.AutoMap();
                                    classMap.MapIdProperty(Key);

                                    BsonClassMap.RegisterClassMap(classMap);
                                }
                            }
                            else
                            {
                                foreach (var t in type.GetTypeInfo().GenericTypeArguments) RegisterGenericChain(t);
                            }
                    }

                    type = type.BaseType;
                }
            }
            catch (Exception e) { Current.Log.Add(e, "Error registering class " + type?.Name + ": " + e.Message); }
        }

        private void ClassMapInitializer(BsonClassMap<MongoDbAdapter> cm)
        {
            cm.AutoMap();
            cm.MapIdMember(c => c.Key);
        }

        #endregion

        #region Interceptor calls

        public override T Get<T>(string key, Mutator mutator = null)
        {
            try
            {
                var filter = Builders<BsonDocument>.Filter.Eq("_id", key);
                var collection = TargetCollection(mutator).Find(filter).ToList();

                var model = collection.FirstOrDefault();

                if (model == null)
                {
                    var isNumeric = long.TryParse(key, out var n);

                    if (isNumeric)
                    {
                        filter = Builders<BsonDocument>.Filter.Eq("_id", n);
                        collection = TargetCollection(mutator).Find(filter).ToList();
                        model = collection.FirstOrDefault();
                    }
                }

                return model == null ? null : BsonSerializer.Deserialize<T>(model);
            }
            catch (Exception e)
            {
                Current.Log.Add($"{Database.Client.Settings.Credential.Username}@{Database.DatabaseNamespace} - {Collection.CollectionNamespace}:{key} {e.Message}", Message.EContentType.Warning);
                throw;
            }
        }

        public override IEnumerable<T> Get<T>(IEnumerable<string> keys, Mutator mutator = null)
        {
            var filter = Builders<BsonDocument>.Filter.In("_id", keys);
            var col = TargetCollection(mutator).Find(filter).ToList();
            var res = col.Select(i => BsonSerializer.Deserialize<T>(i)).ToList();
            return res;
        }

        public override IEnumerable<T> Query<T>(string statement) => Query<T>(statement.ToModifier());
        public override IEnumerable<T> Query<T>(Mutator mutator = null) => Query<T, T>(mutator);
        public override IEnumerable<TU> Query<T, TU>(string statement) => Query<T, TU>(statement.ToModifier());

        public override IEnumerable<TU> Query<T, TU>(Mutator mutator = null)
        {
            var fluentCollection = TargetCollection(mutator).ApplyTransform<T>(mutator?.Transform);

            var colRes = fluentCollection.ToListAsync();
            Task.WhenAll(colRes);

            var res = colRes.Result.AsParallel().Select(v => BsonSerializer.Deserialize<TU>(v)).ToList();
            return res;
        }

        public override long Count<T>(Mutator mutator = null) => TargetCollection(mutator).CountDocuments(mutator?.Transform?.ToBsonQuery() ?? new BsonDocument());

        public override T Insert<T>(T model, Mutator mutator = null) => Upsert(model, mutator);

        public override T Save<T>(T model, Mutator mutator = null) => Upsert(model, mutator);

        public override T Upsert<T>(T obj, Mutator mutator = null)
        {
            string id = null;
            var isNew = false;

            var targetCollection = TargetCollection(mutator);

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
                    var probe = isNew ? null : Get<T>(id, mutator);
                    var document = BsonSerializer.Deserialize<BsonDocument>(obj.ToJson());
                    if (probe == null) { targetCollection.InsertOne(document); }
                    else
                    {
                        var filter = Builders<BsonDocument>.Filter.Eq("_id", id);
                        targetCollection.ReplaceOne(filter, document);
                    }
                }

                return obj;
            }
            catch (Exception e)
            {
                Current.Log.Warn<T>($"{Database.Client.Settings?.Credential?.Username}@{Database?.DatabaseNamespace} - {Collection.CollectionNamespace}:{id} {e.Message}");
                Current.Log.Warn<T>($"{obj.ToJson()}");
                throw;
            }
        }

        private readonly Dictionary<string, IMongoCollection<BsonDocument>> _collectionCache = new Dictionary<string, IMongoCollection<BsonDocument>>();
        private readonly string _defaultCollectionKey = "$DefaultStaticCollection";

        private IMongoCollection<BsonDocument> TargetCollection(Mutator mutator = null)
        {
            var referenceCode = mutator?.SetCode ?? _defaultCollectionKey;

            if (_collectionCache.ContainsKey(referenceCode)) return _collectionCache[referenceCode];

            lock (_collectionCache)
            {
                if (_collectionCache.ContainsKey(referenceCode)) return _collectionCache[referenceCode];

                _collectionCache[referenceCode]
                    = Database.GetCollection<BsonDocument>(ReferenceCollectionName + (referenceCode != _defaultCollectionKey ? $"#{referenceCode}" : ""));
                return _collectionCache[referenceCode];
            }
        }

        public override IEnumerable<T> BulkInsert<T>(IEnumerable<T> models, Mutator mutator = null) => BulkUpsert(models, mutator);

        public override IEnumerable<T> BulkSave<T>(IEnumerable<T> models, Mutator mutator = null) => BulkUpsert(models, mutator);

        public override IEnumerable<T> BulkUpsert<T>(IEnumerable<T> models, Mutator mutator = null)
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
                        { IsUpsert = true });
                    returnModelBuffer.Add(i);
                }

                TargetCollection(mutator).BulkWrite(replacementBuffer.ToArray());

                return returnModelBuffer;
            }
            catch (Exception e)
            {
                Current.Log.Warn<T>($"{Collection.CollectionNamespace}:BulkSave {c}/{enumerable.Count} items: {e.Message}");
                Current.Log.Warn<T>($"{current.ToJson()}");
                Current.Log.Add<T>(e);
                throw;
            }
        }

        public override void BulkRemove<T>(IEnumerable<string> keys, Mutator mutator = null)
        {
            var filter = Builders<BsonDocument>.Filter.In("_id", keys);
            TargetCollection(mutator).DeleteMany(filter);
        }

        public override void BulkRemove<T>(IEnumerable<T> models, Mutator mutator = null) { BulkRemove<T>(models.Select(i => i.GetDataKey()), mutator); }

        public override void Remove<T>(string locator, Mutator mutator = null)
        {
            var filter = Builders<BsonDocument>.Filter.Eq("_id", locator);
            TargetCollection(mutator).DeleteOne(filter);
        }

        public override void Remove<T>(T model, Mutator mutator = null) => Remove<T>(model.GetDataKey(), mutator);

        public override void RemoveAll<T>(Mutator mutator = null) => TargetCollection(mutator).DeleteMany(FilterDefinition<BsonDocument>.Empty);

        #endregion
    }
}