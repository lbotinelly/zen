using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Zen.Base.Module.Data.Adapter
{
    public interface IInterceptor
    {
        #region Bootstrap

        void Initialize<T>() where T : Data<T>;
        void Setup<T>(Settings settings) where T : Data<T>;

        #endregion

        #region Read

        T Get<T>(string key, Mutator mutator = null) where T : Data<T>;
        IEnumerable<T> Get<T>(IEnumerable<string> keys, Mutator mutator = null) where T : Data<T>;

        IEnumerable<T> Query<T>(string statement) where T : Data<T>;
        IEnumerable<T> Query<T>(Mutator mutator = null) where T : Data<T>;

        IEnumerable<TU> Query<T, TU>(string statement) where T : Data<T>;
        IEnumerable<TU> Query<T, TU>(Mutator mutator = null) where T : Data<T>;

        IEnumerable<T> Where<T>(Expression<Func<T, bool>> predicate, Mutator mutator = null) where T : Data<T>;

        long Count<T>(Mutator mutator = null) where T : Data<T>;

        #endregion

        #region Change 

        T Insert<T>(T model, Mutator mutator = null) where T : Data<T>;
        T Save<T>(T model, Mutator mutator = null) where T : Data<T>;
        T Upsert<T>(T model, Mutator mutator = null) where T : Data<T>;

        void Remove<T>(string key, Mutator mutator = null) where T : Data<T>;
        void Remove<T>(T model, Mutator mutator = null) where T : Data<T>;
        void RemoveAll<T>(Mutator mutator = null) where T : Data<T>;

        IEnumerable<T> BulkInsert<T>(IEnumerable<T> models, Mutator mutator = null) where T : Data<T>;
        IEnumerable<T> BulkSave<T>(IEnumerable<T> models, Mutator mutator = null) where T : Data<T>;
        IEnumerable<T> BulkUpsert<T>(IEnumerable<T> models, Mutator mutator = null) where T : Data<T>;
        void BulkRemove<T>(IEnumerable<string> keys, Mutator mutator = null) where T : Data<T>;
        void BulkRemove<T>(IEnumerable<T> models, Mutator mutator = null) where T : Data<T>;

        #endregion

        #region Execute
        //IEnumerable<T> Do<T>(InterceptorQuery.EOperation pOperation, object query, object parm = null);
        #endregion

        #region Set
        void DropSet<T>(string setIdentifier) where T : Data<T>;
        void CopySet<T>(string sourceSetIdentifier, string targetSetIdentifier, bool flushDestination = false) where T : Data<T>;

        #endregion

    }
}