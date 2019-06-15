using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LiteDB;
using Zen.Base;
using Zen.Base.Extension;
using Zen.Base.Module;
using Zen.Base.Module.Data;
using Zen.Base.Module.Data.Connection;

namespace Zen.Module.Data.LiteDB
{
    public class LiteDbInterceptor : IInterceptor
    {
        private LiteDatabase _dbRef;
        private Settings _settings;
        private object _dbCol;

        public void Setup<T>(Settings settings) where T : Data<T>
        {
            _settings = settings;
            _dbRef = new LiteDatabase(settings.ConnectionString ?? $"{Configuration.DataDirectory}{Path.PathSeparator}storage.LiteDB");
        }

        public void Initialize<T>() where T : Data<T>
        {
            _dbCol = _dbRef.GetCollection<T>(_settings.StorageName);
        }

        public T Get<T>(string locator) where T : Data<T> => ((LiteCollection<T>)_dbCol).FindById(locator);
        public IEnumerable<T> Get<T>(IEnumerable<string> keys)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<T> Query<T>(string statement, object rawObject) where T : Data<T>
        {
            throw new NotImplementedException();
        }

        public IEnumerable<T> All<T>(string extraParms = null) where T : Data<T> => ((LiteCollection<T>)_dbCol).FindAll();
        public IEnumerable<TU> All<T, TU>(string extraParms = null) where T : Data<T> => ((LiteCollection<T>)_dbCol).FindAll().Select(i => i.ToType<TU, T>());
        public IEnumerable<T> All<T>(QueryPayload payload, string statement = null) where T : Data<T>
        {
            throw new NotImplementedException();
        }

        public IEnumerable<TU> All<T, TU>(QueryPayload payload, string statement = null) where T : Data<T>
        {
            throw new NotImplementedException();
        }

        public IEnumerable<T> ReferenceQueryByField<T>(string field, string identifier) where T : Data<T>
        {
            throw new NotImplementedException();
        }

        public IEnumerable<T> ReferenceQueryByField<T>(object query) where T : Data<T>
        {
            throw new NotImplementedException();
        }

        public long Count<T>() where T : Data<T>
        {
            throw new NotImplementedException();
        }

        public long Count<T>(string statement) where T : Data<T>
        {
            throw new NotImplementedException();
        }

        public long Count<T>(QueryPayload payload) where T : Data<T>
        {
            throw new NotImplementedException();
        }

        public long Count<T>(QueryPayload payload, string statement) where T : Data<T>
        {
            throw new NotImplementedException();
        }

        public void Insert<T>(Data<T> data) where T : Data<T> { throw new NotImplementedException(); }
        public void Save<T>(Data<T> model) where T : Data<T>
        {
            throw new NotImplementedException();
        }

        public string Upsert<T>(Data<T> data) where T : Data<T>
        {
            if (string.IsNullOrEmpty(Info<T>.GetIdentifier(data)))
                data.SetDataIdentifier(Guid.NewGuid().ToString());

            ((LiteCollection<T>)_dbCol).Upsert(data.ToJson().FromJson<T>());
            return Info<T>.GetIdentifier(data);
        }

        public void BulkSave<T>(IEnumerable<T> source) where T : Data<T>
        {
            foreach (var data in source) { data.Save(); }
        }

        public void Remove<T>(string key) where T : Data<T>
        {
            throw new NotImplementedException();
        }

        public void Remove<T>(Data<T> model) where T : Data<T>
        {
            throw new NotImplementedException();
        }

        public void RemoveAll<T>() where T : Data<T>
        {
            throw new NotImplementedException();
        }
    }
}