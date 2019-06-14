using System.Collections.Generic;
using Zen.Base.Module.Data;
using Zen.Base.Module.Data.Connection;

namespace Zen.Base.Module
{
    public interface IInterceptor
    {
        #region Bootstrap
        void Initialize<T>() where T : Data<T>;
        void Setup<T>(Settings settings) where T : Data<T>;
        #endregion

        #region Read
        T Get<T>(string locator) where T : Data<T>;
        IEnumerable<T> Get<T>(IEnumerable<string> identifiers);

        IEnumerable<T> Query<T>(string sqlStatement, object rawObject) where T : Data<T>;
        IEnumerable<TU> Query<T, TU>(string statement, object rawObject, InterceptorQuery.EType ptype) where T : Data<T>;
        IEnumerable<TU> Query<T, TU>(string statement, object rawObject, InterceptorQuery.EType ptype, InterceptorQuery.EOperation pOperation) where T : Data<T>;

        IEnumerable<T> All<T>(string extraParms = null) where T : Data<T>;
        IEnumerable<TU> All<T, TU>(string extraParms = null) where T : Data<T>;
        IEnumerable<T> All<T>(DataParametrizedGet parm, string extraParms = null) where T : Data<T>;
        IEnumerable<TU> All<T, TU>(DataParametrizedGet parm, string extraParms = null) where T : Data<T>;

        IEnumerable<T> ReferenceQueryByField<T>(string field, string id) where T : Data<T>;
        IEnumerable<T> ReferenceQueryByField<T>(object query) where T : Data<T>;

        long RecordCount<T>() where T : Data<T>;
        long RecordCount<T>(string extraParms) where T : Data<T>;
        long RecordCount<T>(DataParametrizedGet qTerm) where T : Data<T>;
        long RecordCount<T>(DataParametrizedGet qTerm, string extraParms) where T : Data<T>;
        #endregion

        #region Change 
        void Insert<T>(Data<T> data) where T : Data<T>;
        string Save<T>(Data<T> data) where T : Data<T>;
        void BulkSave<T>(IEnumerable<T> source) where T : Data<T>;
        #endregion

        #region Remove

        void Remove<T>(string locator) where T : Data<T>;
        void Remove<T>(Data<T> data) where T : Data<T>;
        void RemoveAll<T>() where T : Data<T>;

        #endregion

        #region Execute
        IEnumerable<T> Do<T>(InterceptorQuery.EOperation pOperation, object query, object parm = null);
        #endregion
    }
}