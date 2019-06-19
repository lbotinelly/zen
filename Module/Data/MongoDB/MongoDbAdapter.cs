using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.IO;
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

        private string _keyMember;
        private object _instance;
        private Type _refType;
        private Settings _statements;
        private DataConfigAttribute _tabledata;
        public IMongoCollection<BsonDocument> Collection;
        public IMongoDatabase Database;
        public string SourceCollection;

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
            try { Collection.Indexes.CreateOne(Builders<BsonDocument>.IndexKeys.Text("$**")); } catch (Exception e)
            {
                var server = Database.Client?.Settings?.Server?.Host;
                try { server = Database?.Client?.Settings?.Servers.FirstOrDefault()?.Host; } catch (Exception) { }

                Current.Log.Warn<T>($"{Database.Client?.Settings?.Credential?.Username}:{Database?.DatabaseNamespace}@{server} - {Collection?.CollectionNamespace} {e.Message} | ERR Creating index {SourceCollection}: {e.Message}");
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

            if (SourceBundle is IStorageContainerResolver) dbname = ((IStorageContainerResolver) SourceBundle).GetStorageContainerName(_statements?.EnvironmentCode);

            if (string.IsNullOrEmpty(dbname)) dbname = "storage";

            // https://jira.mongodb.org/browse/CSHARP-965
            // http://stackoverflow.com/questions/19521626/mongodb-convention-packs

            var pack = new ConventionPack {new IgnoreExtraElementsConvention(true)};

            ConventionRegistry.Register("ignore extra elements", pack, t => true);
            ConventionRegistry.Register("DictionaryRepresentationConvention", new ConventionPack {new DictionaryRepresentationConvention(DictionaryRepresentation.ArrayOfArrays)}, _ => true);
            ConventionRegistry.Register("EnumStringConvention", new ConventionPack {new EnumRepresentationConvention(BsonType.String)}, t => true);

            Database = _client.GetDatabase(dbname);

            Current.Log.Add($"{typeof(T).FullName} {Database.Client?.Settings?.Credential?.Username}:{Database?.DatabaseNamespace}@{server} - REGISTERING", Message.EContentType.StartupSequence);

            _statements = Data<T>.Info<T>.Settings;
            _tabledata = Data<T>.Info<T>.Configuration;

            RegisterGenericChain(typeof(T));

            SetSourceCollection();
        }

        #region Adapter-specific

        private void SetSourceCollection()
        {
            string s;

            if (typeof(IStorageCollectionResolver).IsAssignableFrom(_refType))
            {
                _instance = _refType.GetConstructor(new Type[] { }).Invoke(new object[] { });
                s = ((IStorageCollectionResolver) _instance).GetStorageCollectionName();
                Current.Log.Add("MongoDbinterceptor.SetSourceCollection: CUSTOM_INIT " + s, Message.EContentType.StartupSequence);
            }
            else
            {
                s = _refType.FullName;

                if (!string.IsNullOrEmpty(_tabledata?.TablePrefix)) s = _tabledata.TablePrefix + "." + _refType.Name;
                if (!string.IsNullOrEmpty(_tabledata?.TableName)) s = _tabledata.TableName;
            }

            SourceCollection = _tabledata?.IgnoreEnvironmentPrefix == true ? s : _statements.EnvironmentCode + "." + s;
            SetCollection();
        }

        private void SetCollection() { Collection = Database.GetCollection<BsonDocument>(SourceCollection); }

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
                                    var classMap = (BsonClassMap) Activator.CreateInstance(classMapType);

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
            } catch (Exception e) { Current.Log.Add(e, "Error registering class " + type?.Name + ": " + e.Message); }
        }

        private void ClassMapInitializer(BsonClassMap<MongoDbAdapter> cm)
        {
            cm.AutoMap();
            cm.MapIdMember(c => c.Key);
        }

        #endregion

        #region Interceptor calls

        public override T Get<T>(string key)
        {
            try
            {
                var filter = Builders<BsonDocument>.Filter.Eq("_id", key);
                var col = Collection.Find(filter).ToList();
                var target = col.FirstOrDefault();

                if (target == null)
                {
                    var isNumeric = long.TryParse(key, out var n);

                    if (isNumeric)
                    {
                        filter = Builders<BsonDocument>.Filter.Eq("_id", n);
                        col = Collection.Find(filter).ToList();
                        target = col.FirstOrDefault();
                    }
                }

                return target == null ? null : BsonSerializer.Deserialize<T>(target);
            } catch (Exception e)
            {
                Current.Log.Add($"{Database.Client.Settings.Credential.Username}@{Database.DatabaseNamespace} - {Collection.CollectionNamespace}:{key} {e.Message}", Message.EContentType.Warning);
                throw;
            }
        }

        public override IEnumerable<T> Get<T>(IEnumerable<string> keys)
        {
            var filter = Builders<BsonDocument>.Filter.In("_id", keys);
            var col = Collection.Find(filter).ToList();
            var res = col.Select(i => BsonSerializer.Deserialize<T>(i)).ToList();
            return res;
        }

        public override IEnumerable<T> Query<T>(string statement) { return Query<T>(statement.ToModifier()); }
        public override IEnumerable<T> Query<T>(QueryModifier modifier = null) { return Query<T, T>(modifier); }
        public override IEnumerable<TU> Query<T, TU>(string statement) { return Query<T, TU>(statement.ToModifier()); }

        public override IEnumerable<TU> Query<T, TU>(QueryModifier modifier = null)
        {
            //var rawQuery = modifier?.Payload?.ToBsonQuery() ?? new BsonDocument();
            //var col = Collection.Find(rawQuery).ToEnumerable();
            //var transform = col.AsParallel().Select(a => BsonSerializer.Deserialize<TU>(a)).ToList();
            //return transform;

            var payload = modifier?.Payload;

            var queryFilter = payload?.ToBsonQuery() ?? new BsonDocument();
            var querySort = payload?.ToBsonFilter();

            SortDefinition<BsonDocument> sortFilter = querySort;

            if (_tabledata?.AutoGenerateMissingSchema == true)
                if (payload?.OrderBy != null)
                    Collection.Indexes.CreateOne(querySort?.ToJson(new JsonWriterSettings {OutputMode = JsonOutputMode.Strict}));

            IFindFluent<BsonDocument, BsonDocument> fluentCollection;

            try
            {
                fluentCollection = Collection.Find(queryFilter);
                if (sortFilter != null) fluentCollection = fluentCollection.Sort(sortFilter);
            } catch (Exception e)
            {
                Current.Log.Warn<T>($"{Database.Client?.Settings?.Credential?.Username}@{Database?.DatabaseNamespace} - {Collection?.CollectionNamespace} {e.Message}");
                Current.Log.Warn<T>($"{payload?.ToJson()}");
                Current.Log.Add<T>(e);
                throw;
            }

            if (payload?.PageSize != null || payload?.PageIndex != null)
                fluentCollection
                    .Skip((int) ((payload?.PageIndex ?? 0) * (payload?.PageSize ?? 40)))
                    .Limit((int) (payload?.PageSize ?? 40))
                    ;

            var colRes = fluentCollection.ToListAsync();
            Task.WhenAll(colRes);

            var res = colRes.Result.AsParallel().Select(v => BsonSerializer.Deserialize<TU>(v)).ToList();
            return res;
        }

        public override long Count<T>(string statement) { return Count<T>(statement.ToModifier()); }

        public override long Count<T>(QueryModifier modifier = null) { return Collection.CountDocuments(modifier?.Payload?.ToBsonQuery() ?? new BsonDocument()); }

        public override T Insert<T>(T model) { return Upsert(model); }

        public override T Save<T>(T model) { return Upsert(model); }

        public override T Upsert<T>(T obj)
        {
            string id = null;
            var isNew = false;

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
                    var probe = isNew ? null : Get<T>(id);

                    var document = BsonSerializer.Deserialize<BsonDocument>(obj.ToJson());

                    if (probe == null) { Collection.InsertOne(document); }
                    else
                    {
                        var filter = Builders<BsonDocument>.Filter.Eq("_id", id);
                        Collection.ReplaceOne(filter, document);
                    }
                }

                return obj;
            } catch (Exception e)
            {
                Current.Log.Warn<T>($"{Database.Client.Settings?.Credential?.Username}@{Database?.DatabaseNamespace} - {Collection.CollectionNamespace}:{id} {e.Message}");
                Current.Log.Warn<T>($"{obj.ToJson()}");
                throw;
            }
        }

        public override IEnumerable<T> BulkInsert<T>(IEnumerable<T> models) => BulkUpsert(models);

        public override IEnumerable<T> BulkSave<T>(IEnumerable<T> models) => BulkUpsert(models);

        public override IEnumerable<T> BulkUpsert<T>(IEnumerable<T> models)
        {
            var c = 0;

            if (models == null) return null;

            var enumerable = models.ToList();

            if (!enumerable.Any()) return null;

            T current = null;

            try
            {
                var replacementBuffer = new List<ReplaceOneModel<BsonDocument>>();
                var outBuffer = new List<T>();

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
                    outBuffer.Add(i);
                }

                Collection.BulkWrite(replacementBuffer.ToArray());
                return outBuffer;
            } catch (Exception e)
            {
                Current.Log.Warn<T>($"{Collection.CollectionNamespace}:BulkSave {c}/{enumerable.Count} items ERR {e.Message}");
                Current.Log.Warn<T>($"{current.ToJson()}");
                Current.Log.Add<T>(e);
                throw;
            }
        }

        public override void BulkRemove<T>(IEnumerable<string> keys)
        {
            var filter = Builders<BsonDocument>.Filter.In("_id", keys);
            Collection.DeleteMany(filter);
        }
        public override void BulkRemove<T>(IEnumerable<T> models) => BulkRemove<T>(models.Select(i => i.GetDataKey()));

        public override void Remove<T>(string locator)
        {
            var filter = Builders<BsonDocument>.Filter.Eq("_id", locator);
            Collection.DeleteOne(filter);
        }

        public override void Remove<T>(T model) => Remove<T>(model.GetDataKey());

        public override void RemoveAll<T>() { Collection.DeleteMany(FilterDefinition<BsonDocument>.Empty); }

        #endregion
    }
}