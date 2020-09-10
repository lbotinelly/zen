using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using LiteDB;
using Zen.Base;
using Zen.Base.Extension;
using Zen.Base.Module;
using Zen.Base.Module.Data;
using Zen.Base.Module.Data.Adapter;
using Zen.Base.Module.Data.Connection;

namespace Zen.Module.Data.LiteDB
{
    public class LiteDbAdapter<T> : DataAdapterPrimitive<T> where T : Data<T>
    {
        private readonly string _collectionPrefix = "";
        private readonly string _collectionPrefixSeparator = "#";
        private ILiteCollection<T> _collection;
        private string _collectionName = "";
        private string _collectionNamespace = "";
        private Type _refType;
        private Settings<T> _statements;
        private DataConfigAttribute _tabledata;
        public LiteDatabase Database;

        public override void Setup(Settings<T> settings)
        {
            _statements = settings;

            _refType = typeof(T);

            var statementsConnectionString = _statements.ConnectionString ?? $"{Host.DataDirectory}{Path.DirectorySeparatorChar}lite.db";
            Database = Instances.GetDatabase(statementsConnectionString);

            _statements = Info<T>.Settings;
            _tabledata = Info<T>.Configuration;

            var mapper = BsonMapper.Global;

            SetBaseCollectionName();

             _collection = Database.GetCollection<T>(ReferenceCollectionName);
        }

        private ILiteCollection<T> Collection()
        {
            return _collection;
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


        public override T Get(string key, Mutator mutator = null)
        {
            var statement = $"$._id = '{key}'";

            var model = Collection().FindOne(statement);
            return model;
        }

        public override IEnumerable<T> Get(IEnumerable<string> keys, Mutator mutator = null)
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<T> Query(string statement)
        {
            return Collection().Find(statement);
        }

        public override IEnumerable<T> Query(Mutator mutator = null)
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<TU> Query<TU>(string statement)
        {
            var modelBuffer = Collection().Find(statement);
            return modelBuffer.ToJson().FromJson<List<TU>>();
        }

        public override IEnumerable<TU> Query<TU>(Mutator mutator = null) => Query<TU>(mutator.ToQueryExpression());

        public override long Count(Mutator mutator = null) => Collection().Count();
        public override bool KeyExists(string key, Mutator mutator = null)
        {
            if (Get(key) != null) return true;
            return false;
        }

        public override T Insert(T model, Mutator mutator = null)
        {
            Collection().Insert(model);
            return model;
        }

        public override T Save(T model, Mutator mutator = null) => Upsert(model);

        public override T Upsert(T model, Mutator mutator = null)
        {
            Collection().Upsert(model);
            return model;
        }

        public override void Remove(string key, Mutator mutator = null) => Collection().Delete(key);

        public override void Remove(T model, Mutator mutator = null) => Remove(model.GetDataKey(), mutator);

        public override void RemoveAll(Mutator mutator = null) => Collection().DeleteMany("1=1");

        public override IEnumerable<T> BulkInsert(IEnumerable<T> models, Mutator mutator = null)
        {
            var modelBuffer = models.ToList();
            Collection().InsertBulk(modelBuffer);
            return modelBuffer;
        }

        public override IEnumerable<T> BulkSave(IEnumerable<T> models, Mutator mutator = null)
        {
            var modelBuffer = models.ToList();
            foreach (var data in modelBuffer) Save(data);
            return modelBuffer;
        }

        public override IEnumerable<T> BulkUpsert(IEnumerable<T> models, Mutator mutator = null)
        {
            var modelBuffer = models.ToList();
            foreach (var data in modelBuffer) Upsert(data);
            return modelBuffer;
        }

        public override void BulkRemove(IEnumerable<string> keys, Mutator mutator = null)
        {
            var keyBuffer = keys.ToList();
            foreach (var data in keyBuffer) Remove(data);
        }

        public override void BulkRemove(IEnumerable<T> models, Mutator mutator = null)
        {
            var keyBuffer = models.Select(i => i.GetDataKey()).ToList();
            foreach (var data in keyBuffer) Remove(data);
        }

        public override void DropSet(string setName)
        {
            throw new NotImplementedException();
        }

        public override void CopySet(string sourceSetIdentifier, string targetSetIdentifier, bool flushDestination = false)
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<T> Where(Expression<Func<T, bool>> predicate, Mutator mutator = null)
        {
            return Collection().Find(predicate);
        }

        public override void Initialize() { }
    }
}