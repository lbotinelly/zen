using System;
using System.Collections.Generic;
using Zen.Base.Module;
using Zen.Base.Module.Data;
using Zen.Base.Module.Data.Connection;

namespace Zen.Module.Data.LiteDB
{
    public class LiteDbInterceptor: IInterceptor
    {
        public void Initialize<T>() where T : Data<T> { throw new NotImplementedException(); }
        public void Setup<T>(Settings statements) where T : Data<T> { throw new NotImplementedException(); }
        public void Connect<T>(string statementsConnectionString, ConnectionBundlePrimitive bundle) where T : Data<T> { throw new NotImplementedException(); }
        public T Get<T>(string locator) where T : Data<T> { throw new NotImplementedException(); }
        public List<T> Get<T>(List<string> identifiers) { throw new NotImplementedException(); }
        public List<T> Query<T>(string sqlStatement, object rawObject) where T : Data<T> { throw new NotImplementedException(); }
        public List<TU> Query<T, TU>(string statement, object rawObject, InterceptorQuery.EType ptype) where T : Data<T> { throw new NotImplementedException(); }
        public List<TU> Query<T, TU>(string statement, object rawObject, InterceptorQuery.EType ptype, InterceptorQuery.EOperation pOperation) where T : Data<T> { throw new NotImplementedException(); }
        public List<T> GetAll<T>(string extraParms = null) where T : Data<T> { throw new NotImplementedException(); }
        public List<TU> GetAll<T, TU>(string extraParms = null) where T : Data<T> { throw new NotImplementedException(); }
        public List<T> GetAll<T>(DataParametrizedGet parm, string extraParms = null) where T : Data<T> { throw new NotImplementedException(); }
        public List<TU> GetAll<T, TU>(DataParametrizedGet parm, string extraParms = null) where T : Data<T> { throw new NotImplementedException(); }
        public List<T> ReferenceQueryByField<T>(string field, string id) where T : Data<T> { throw new NotImplementedException(); }
        public List<T> ReferenceQueryByField<T>(object query) where T : Data<T> { throw new NotImplementedException(); }
        public long RecordCount<T>() where T : Data<T> { throw new NotImplementedException(); }
        public long RecordCount<T>(string extraParms) where T : Data<T> { throw new NotImplementedException(); }
        public long RecordCount<T>(DataParametrizedGet qTerm) where T : Data<T> { throw new NotImplementedException(); }
        public long RecordCount<T>(DataParametrizedGet qTerm, string extraParms) where T : Data<T> { throw new NotImplementedException(); }
        public void Insert<T>(Data<T> data) where T : Data<T> { throw new NotImplementedException(); }
        public string Save<T>(Data<T> data) where T : Data<T> { throw new NotImplementedException(); }
        public void BulkSave<T>(List<T> source) where T : Data<T> { throw new NotImplementedException(); }
        public void Remove<T>(string locator) where T : Data<T> { throw new NotImplementedException(); }
        public void Remove<T>(Data<T> data) where T : Data<T> { throw new NotImplementedException(); }
        public void RemoveAll<T>() where T : Data<T> { throw new NotImplementedException(); }
        public List<T> Do<T>(InterceptorQuery.EOperation pOperation, object query, object parm = null) { throw new NotImplementedException(); }
    }
}
