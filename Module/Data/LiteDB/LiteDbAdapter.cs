using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LiteDB;
using Zen.Base;
using Zen.Base.Extension;
using Zen.Base.Module;
using Zen.Base.Module.Data;
using Zen.Base.Module.Data.Adapter;

namespace Zen.Module.Data.LiteDB
{
    public class LiteDbAdapter : DataAdapterPrimitive
    {
        private object _dbCol;

        private LiteDatabase _dbRef;
        private Settings _settings;

        public override void Setup<T>(Settings settings)
        {
            _settings = settings;
            _dbRef = new LiteDatabase(settings.ConnectionString ?? $"{Configuration.DataDirectory}{Path.DirectorySeparatorChar}storage.LiteDB");
        }

        public override void Initialize<T>()
        {
            _dbCol = _dbRef.GetCollection<T>(_settings.StorageName);

            var mapper = BsonMapper.Global;

            if (Data<T>.Info<T>.Settings.KeyProperty == null) Current.Log.Warn<T>("LiteDbAdapter.Initialize: Driver requires a Property to defined as key.");
        }

        public override T Get<T>(string key) { return ((LiteCollection<T>) _dbCol).FindById(key); }

        public override IEnumerable<T> Get<T>(IEnumerable<string> keys)
        {
            var cache = new List<T>();
            foreach (var key in keys) cache.Add(((LiteCollection<T>) _dbCol).FindById(key));

            return cache;
        }

        public override IEnumerable<T> Query<T>(string statement) { return null; }

        public override IEnumerable<T> All<T>(string statement = null) { return ((LiteCollection<T>) _dbCol).FindAll(); }

        public override IEnumerable<TU> All<T, TU>(string statement = null) { return ((LiteCollection<T>) _dbCol).FindAll().Select(i => i.ToType<TU, T>()); }

        public override IEnumerable<T> All<T>(QueryPayload payload, string statement = null) { throw new NotImplementedException(); }

        public override IEnumerable<TU> All<T, TU>(QueryPayload payload, string statement = null) { throw new NotImplementedException(); }

        public override long Count<T>() { throw new NotImplementedException(); }

        public override long Count<T>(string statement) { throw new NotImplementedException(); }

        public override long Count<T>(QueryPayload payload) { throw new NotImplementedException(); }

        public override long Count<T>(QueryPayload payload, string statement) { throw new NotImplementedException(); }

        public override void Insert<T>(Data<T> data) { Upsert(data); }

        public override void Save<T>(Data<T> model) { Upsert(model); }

        public override string Upsert<T>(Data<T> data)
        {
            if (string.IsNullOrEmpty(Info<T>.GetDataKey(data))) data.SetDataKey(Guid.NewGuid().ToString());

            ((LiteCollection<T>) _dbCol).Upsert(data.ToJson().FromJson<T>());
            return Info<T>.GetDataKey(data);
        }

        public override void BulkSave<T>(IEnumerable<T> source)
        {
            foreach (var data in source) data.Save();
        }

        public override void Remove<T>(string key) { throw new NotImplementedException(); }

        public override void Remove<T>(Data<T> model) { throw new NotImplementedException(); }

        public override void RemoveAll<T>() { throw new NotImplementedException(); }
    }
}