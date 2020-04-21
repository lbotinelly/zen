using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using LiteDB;
using Zen.Base;
using Zen.Base.Extension;
using Zen.Base.Module.Data;
using Zen.Base.Module.Data.Adapter;
using Zen.Base.Module.Data.Connection;

namespace Zen.Module.Data.LiteDB
{
    public class LiteDbAdapter : DataAdapterPrimitive
    {
        private readonly string _collectionPrefix = "";
        private readonly string _collectionPrefixSeparator = "#";
        private object _Collection;
        private string _collectionName = "";
        private string _collectionNamespace = "";
        private Type _refType;
        private Settings _statements;
        private DataConfigAttribute _tabledata;
        public LiteDatabase Database;

        public override void Setup<T>(Settings settings)
        {
            _statements = settings;

            _refType = typeof(T);

            var statementsConnectionString = _statements.ConnectionString ?? $"{Host.DataDirectory}{Path.DirectorySeparatorChar}lite.db";
            Database = Instances.GetDatabase(statementsConnectionString);

            _statements = Info<T>.Settings;
            _tabledata = Info<T>.Configuration;

            var mapper = BsonMapper.Global;

            SetBaseCollectionName();

            _Collection = Database.GetCollection<T>(ReferenceCollectionName);
        }

        private ILiteCollection<T> Collection<T>()
        {
            return (ILiteCollection<T>)_Collection;
        }

        private void SetBaseCollectionName()
        {
            _collectionNamespace = _tabledata?.SetPrefix ?? _statements.TypeNamespace;
            var typeName = _tabledata?.SetName ?? _statements.TypeName;
            if (typeof(IStorageCollectionResolver).GetTypeInfo().IsAssignableFrom(_refType.GetTypeInfo()))
                typeName = ((IStorageCollectionResolver)_refType.CreateInstance()).GetStorageCollectionName();
            _collectionName = typeName;
            ReferenceCollectionName = GetCollectionName().Replace(".", "_");
        }

        private string GetCollectionName(string suffix = "")
        {
            var parsedSuffix = !string.IsNullOrEmpty(suffix) ? _collectionPrefixSeparator + suffix : "";
            return $"{_collectionPrefix}{_collectionNamespace}.{_collectionName}{parsedSuffix}";
        }


        public override T Get<T>(string key, Mutator mutator = null)
        {
            var statement = $"$._id = '{key}'";

            var model = Collection<T>().FindOne(statement);
            return model;
        }

        public override IEnumerable<T> Get<T>(IEnumerable<string> keys, Mutator mutator = null)
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<T> Query<T>(string statement)
        {
            return Collection<T>().Find(statement);
        }

        public override IEnumerable<T> Query<T>(Mutator mutator = null)
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<TU> Query<T, TU>(string statement)
        {
            var modelBuffer = Collection<T>().Find(statement);
            return modelBuffer.ToJson().FromJson<List<TU>>();
        }

        public override IEnumerable<TU> Query<T, TU>(Mutator mutator = null) => Query<T, TU>(mutator.ToQueryExpression());

        public override long Count<T>(Mutator mutator = null) => Collection<T>().Count();

        public override T Insert<T>(T model, Mutator mutator = null)
        {
            Collection<T>().Insert(model);
            return model;
        }

        public override T Save<T>(T model, Mutator mutator = null) => Upsert(model);

        public override T Upsert<T>(T model, Mutator mutator = null)
        {
            Collection<T>().Upsert(model);
            return model;
        }

        public override void Remove<T>(string key, Mutator mutator = null) => Collection<T>().Delete(key);

        public override void Remove<T>(T model, Mutator mutator = null) => Remove<T>(model.GetDataKey(), mutator);

        public override void RemoveAll<T>(Mutator mutator = null) => Collection<T>().DeleteMany("1=1");

        public override IEnumerable<T> BulkInsert<T>(IEnumerable<T> models, Mutator mutator = null)
        {
            var modelBuffer = models.ToList();
            Collection<T>().InsertBulk(modelBuffer);
            return modelBuffer;
        }

        public override IEnumerable<T> BulkSave<T>(IEnumerable<T> models, Mutator mutator = null)
        {
            var modelBuffer = models.ToList();
            foreach (var data in modelBuffer) Save(data);
            return modelBuffer;
        }

        public override IEnumerable<T> BulkUpsert<T>(IEnumerable<T> models, Mutator mutator = null)
        {
            var modelBuffer = models.ToList();
            foreach (var data in modelBuffer) Upsert(data);
            return modelBuffer;
        }

        public override void BulkRemove<T>(IEnumerable<string> keys, Mutator mutator = null)
        {
            var keyBuffer = keys.ToList();
            foreach (var data in keyBuffer) Remove<T>(data);
        }

        public override void BulkRemove<T>(IEnumerable<T> models, Mutator mutator = null)
        {
            var keyBuffer = models.Select(i => i.GetDataKey()).ToList();
            foreach (var data in keyBuffer) Remove<T>(data);
        }

        public override void DropSet<T>(string setName)
        {
            throw new NotImplementedException();
        }

        public override void CopySet<T>(string sourceSetIdentifier, string targetSetIdentifier, bool flushDestination = false)
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<T> Where<T>(Expression<Func<T, bool>> predicate, Mutator mutator = null)
        {
            return Collection<T>().Find(predicate);
        }

        public override void Initialize<T>() { }
    }
}