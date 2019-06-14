using System;
using System.Collections.Generic;
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
            _dbRef = new LiteDatabase(settings.ConnectionString ?? Configuration.DataDirectory + "\\liteDB.storage");
        }

        public void Initialize<T>() where T : Data<T>
        {
            _dbCol = _dbRef.GetCollection<T>(_settings.StorageName);
        }

        public T Get<T>(string locator) where T : Data<T> => ((LiteCollection<T>) _dbCol).FindById(locator);
        public IEnumerable<T> Get<T>(IEnumerable<string> identifiers) { throw new NotImplementedException(); }
        public IEnumerable<T> Query<T>(string sqlStatement, object rawObject) where T : Data<T> { throw new NotImplementedException(); }
        public IEnumerable<TU> Query<T, TU>(string statement, object rawObject, InterceptorQuery.EType ptype) where T : Data<T> { throw new NotImplementedException(); }
        public IEnumerable<TU> Query<T, TU>(string statement, object rawObject, InterceptorQuery.EType ptype, InterceptorQuery.EOperation pOperation) where T : Data<T> { throw new NotImplementedException(); }
        public IEnumerable<T> All<T>(string extraParms = null) where T : Data<T> => ((LiteCollection<T>)_dbCol).FindAll();
        public IEnumerable<TU> All<T, TU>(string extraParms = null) where T : Data<T> => ((LiteCollection<T>)_dbCol).FindAll().Select(i => i.ToType<TU, T>());

        public IEnumerable<T> All<T>(DataParametrizedGet parm, string extraParms = null) where T : Data<T> { throw new NotImplementedException(); }
        public IEnumerable<TU> All<T, TU>(DataParametrizedGet parm, string extraParms = null) where T : Data<T> { throw new NotImplementedException(); }
        public IEnumerable<T> ReferenceQueryByField<T>(string field, string id) where T : Data<T> { throw new NotImplementedException(); }
        public IEnumerable<T> ReferenceQueryByField<T>(object query) where T : Data<T> { throw new NotImplementedException(); }
        public long RecordCount<T>() where T : Data<T> { throw new NotImplementedException(); }
        public long RecordCount<T>(string extraParms) where T : Data<T> { throw new NotImplementedException(); }
        public long RecordCount<T>(DataParametrizedGet qTerm) where T : Data<T> { throw new NotImplementedException(); }
        public long RecordCount<T>(DataParametrizedGet qTerm, string extraParms) where T : Data<T> { throw new NotImplementedException(); }
        public void Insert<T>(Data<T> data) where T : Data<T> { throw new NotImplementedException(); }

        public string Save<T>(Data<T> data) where T : Data<T>
        {
            if (string.IsNullOrEmpty(Info<T>.GetIdentifier(data)))
                data.SetDataIdentifier(Guid.NewGuid().ToString());

            ((LiteCollection < T >)_dbCol).Upsert(data.ToJson().FromJson<T>());
            return Info<T>.GetIdentifier(data);
        }
        public void BulkSave<T>(IEnumerable<T> source) where T : Data<T> { throw new NotImplementedException(); }
        public void Remove<T>(string locator) where T : Data<T> { throw new NotImplementedException(); }
        public void Remove<T>(Data<T> data) where T : Data<T> { throw new NotImplementedException(); }
        public void RemoveAll<T>() where T : Data<T> { throw new NotImplementedException(); }
        public IEnumerable<T> Do<T>(InterceptorQuery.EOperation pOperation, object query, object parm = null) { throw new NotImplementedException(); }
        public void Connect<T>(string statementsConnectionString, ConnectionBundlePrimitive bundle) where T : Data<T> { throw new NotImplementedException(); }
    }
}