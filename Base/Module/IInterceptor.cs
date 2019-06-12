using System.Collections.Generic;
using Zen.Base.Module.Data.Connection;

namespace Zen.Base.Module
{
    public interface IInterceptor
    {
        #region Bootstrap
        void Initialize<T>() where T : Data<T>;
        void Setup<T>(Data.Settings statements) where T : Data<T>;
        void Connect<T>(string statementsConnectionString, ConnectionBundlePrimitive bundle) where T : Data<T>;
        #endregion

        #region Read
        T Get<T>(string locator) where T : Data<T>;
        List<T> Get<T>(List<string> identifiers);

        List<T> Query<T>(string sqlStatement, object rawObject) where T : Data<T>;
        List<TU> Query<T, TU>(string statement, object rawObject, InterceptorQuery.EType ptype) where T : Data<T>;
        List<TU> Query<T, TU>(string statement, object rawObject, InterceptorQuery.EType ptype, InterceptorQuery.EOperation pOperation) where T : Data<T>;

        List<T> GetAll<T>(string extraParms = null) where T : Data<T>;
        List<TU> GetAll<T, TU>(string extraParms = null) where T : Data<T>;
        List<T> GetAll<T>(DataParametrizedGet parm, string extraParms = null) where T : Data<T>;
        List<TU> GetAll<T, TU>(DataParametrizedGet parm, string extraParms = null) where T : Data<T>;

        List<T> ReferenceQueryByField<T>(string field, string id) where T : Data<T>;
        List<T> ReferenceQueryByField<T>(object query) where T : Data<T>;

        long RecordCount<T>() where T : Data<T>;
        long RecordCount<T>(string extraParms) where T : Data<T>;
        long RecordCount<T>(DataParametrizedGet qTerm) where T : Data<T>;
        long RecordCount<T>(DataParametrizedGet qTerm, string extraParms) where T : Data<T>;
        #endregion

        #region Change 
        void Insert<T>(Data<T> data) where T : Data<T>;
        string Save<T>(Data<T> data) where T : Data<T>;
        void BulkSave<T>(List<T> source) where T : Data<T>;
        #endregion

        #region Remove

        void Remove<T>(string locator) where T : Data<T>;
        void Remove<T>(Data<T> data) where T : Data<T>;
        void RemoveAll<T>() where T : Data<T>;

        #endregion

        #region Execute
        List<T> Do<T>(InterceptorQuery.EOperation pOperation, object query, object parm = null);
        #endregion
    }
}