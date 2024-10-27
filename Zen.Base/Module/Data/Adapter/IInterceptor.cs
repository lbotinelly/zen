using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Zen.Base.Module.Data.Adapter
{
    public interface IInterceptor<T> where T : Data<T>
    {
        #region Bootstrap

        void Initialize();
        void Setup(Settings<T> settings);

        #endregion

        #region Read
        T Get(string key, Mutator mutator = null);
        IEnumerable<T> Get(IEnumerable<string> keys, Mutator mutator = null);
        IEnumerable<T> Query(string statement);
        IEnumerable<T> Query(Mutator mutator = null);
        IEnumerable<TU> Query<TU>(string statement);
        IEnumerable<TU> Query<TU>(Mutator mutator = null);
        IEnumerable<T> Where(Expression<Func<T, bool>> predicate, Mutator mutator = null);
        long Count(Mutator mutator = null);
        #endregion

        #region Change 
        T Insert(T model, Mutator mutator = null);
        T Save(T model, Mutator mutator = null);
        T Upsert(T model, Mutator mutator = null);

        void Remove(string key, Mutator mutator = null);
        void Remove(T model, Mutator mutator = null);
        void RemoveAll(Mutator mutator = null);

        IEnumerable<T> BulkInsert(IEnumerable<T> models, Mutator mutator = null);
        IEnumerable<T> BulkSave(IEnumerable<T> models, Mutator mutator = null);
        IEnumerable<T> BulkUpsert(IEnumerable<T> models, Mutator mutator = null);
        void BulkRemove(IEnumerable<string> keys, Mutator mutator = null);
        void BulkRemove(IEnumerable<T> models, Mutator mutator = null);

        #endregion

        #region Execute
        //IEnumerable<T> Do(InterceptorQuery.EOperation pOperation, object query, object parm = null);
        #endregion
        #region Set
        void DropSet(string setIdentifier);
        void CopySet(string sourceSetIdentifier, string targetSetIdentifier, bool flushDestination = false);
        #endregion
    }
}