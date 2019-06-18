using System.Collections.Generic;

namespace Zen.Base.Module.Data.Adapter
{
    public interface IInterceptor
    {
        #region Bootstrap
        void Initialize<T>() where T : Data<T>;
        void Setup<T>(Settings settings) where T : Data<T>;
        #endregion

        #region Read
        T Get<T>(string key) where T : Data<T>;
        IEnumerable<T> Get<T>(IEnumerable<string> keys) where T : Data<T>;

        IEnumerable<T> Query<T>(string statement) where T : Data<T>;
        IEnumerable<T> Query<T>(QueryModifier modifier = null) where T : Data<T>;
        IEnumerable<TU> Query<T, TU>(string statement) where T : Data<T>;
        IEnumerable<TU> Query<T, TU>(QueryModifier modifier = null) where T : Data<T>;

        long Count<T>(string statement) where T : Data<T>;
        long Count<T>(QueryModifier modifier = null) where T : Data<T>;
        #endregion

        #region Change 
        T Insert<T>(T model) where T : Data<T>;
        T Save<T>(T model) where T : Data<T>;
        T Upsert<T>(T model) where T : Data<T>;

        IEnumerable<T> BulkInsert<T>(IEnumerable<T> models) where T : Data<T>;
        IEnumerable<T> BulkSave<T>(IEnumerable<T> models) where T : Data<T>;
        IEnumerable<T> BulkUpsert<T>(IEnumerable<T> models) where T : Data<T>;

        void Remove<T>(string key) where T : Data<T>;
        void Remove<T>(T model) where T : Data<T>;
        void RemoveAll<T>() where T : Data<T>;
        #endregion

        #region Execute
        //IEnumerable<T> Do<T>(InterceptorQuery.EOperation pOperation, object query, object parm = null);
        #endregion
    }
}