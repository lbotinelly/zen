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
        T Get<T>(string key) where T : Data<T>;
        IEnumerable<T> Get<T>(IEnumerable<string> keys);

        IEnumerable<T> Query<T>(string statement) where T : Data<T>;

        IEnumerable<T> All<T>(string statement = null) where T : Data<T>;
        IEnumerable<TU> All<T, TU>(string statement = null) where T : Data<T>;
        IEnumerable<T> All<T>(QueryPayload payload, string statement = null) where T : Data<T>;
        IEnumerable<TU> All<T, TU>(QueryPayload payload, string statement = null) where T : Data<T>;

        long Count<T>() where T : Data<T>;
        long Count<T>(string statement) where T : Data<T>;
        long Count<T>(QueryPayload payload) where T : Data<T>;
        long Count<T>(QueryPayload payload, string statement) where T : Data<T>;
        #endregion

        #region Change 
        void Insert<T>(Data<T> model) where T : Data<T>;
        void Save<T>(Data<T> model) where T : Data<T>;
        string Upsert<T>(Data<T> model) where T : Data<T>;
        void BulkSave<T>(IEnumerable<T> models) where T : Data<T>;
        #endregion

        #region Remove

        void Remove<T>(string key) where T : Data<T>;
        void Remove<T>(Data<T> model) where T : Data<T>;
        void RemoveAll<T>() where T : Data<T>;

        #endregion

        #region Execute
        //IEnumerable<T> Do<T>(InterceptorQuery.EOperation pOperation, object query, object parm = null);
        #endregion
    }
}