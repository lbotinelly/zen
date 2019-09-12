using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using Dapper;
using Zen.Base.Extension;
using Zen.Base.Module;
using Zen.Base.Module.Data;
using Zen.Base.Module.Data.Adapter;

namespace Zen.Module.Data.Relational
{
    public abstract class RelationalAdapter : DataAdapterPrimitive, IRelationalStatements
    {
        public static Dictionary<Type, Dictionary<string, string>> TypeMaps = new Dictionary<Type, Dictionary<string, string>>();

        #region Custom Members

        public List<T> AdapterQuery<T>(string statement, Mutator mutator = null) where T : Data<T>
        {
            var builder = mutator.ToSqlBuilderTemplate();

            try
            {
                using (var conn = GetConnection<T>())
                {
                    conn.Open();

                    var o = conn.Query(builder.RawSql, builder.Parameters)
                        .Select(a => (IDictionary<string, object>) a)
                        .ToList();
                    conn.Close();

                    var ret = o.Select(refObj => refObj.GetObject<T>(TypeMaps[typeof(T)])).ToList();

                    return ret;
                }
            } catch (Exception e) { throw new DataException(Data<T>.Info<T>.Settings.TypeQualifiedName + " Entity/Dapper Query: Error while issuing statements to the database. Error:  [" + e.Message + "].", e); }
        }

        #endregion

        #region Implementation of IRelationalStatements

        public virtual bool UseIndependentStatementsForKeyExtraction { get; } = false;
        public virtual bool UseNumericPrimaryKeyOnly { get; } = false;
        public virtual bool UseOutputParameterForInsertedKeyExtraction { get; } = false;
        public virtual RelationalStatements Statements { get; } = new RelationalStatements();

        public virtual DbConnection GetConnection<T>() where T : Data<T> { return null; }

        #endregion

        #region Overrides of DataAdapterPrimitive

        public override void Setup<T>(Settings settings) { throw new NotImplementedException(); }
        public override void Initialize<T>() { throw new NotImplementedException(); }

        public override T Get<T>(string key, Mutator mutator = null) { throw new NotImplementedException(); }
        public override IEnumerable<T> Get<T>(IEnumerable<string> keys, Mutator mutator = null) { throw new NotImplementedException(); }
        public override IEnumerable<T> Query<T>(string statement) { throw new NotImplementedException(); }
        public override IEnumerable<T> Query<T>(Mutator mutator = null) { throw new NotImplementedException(); }
        public override IEnumerable<TU> Query<T, TU>(string statement) { throw new NotImplementedException(); }
        public override IEnumerable<TU> Query<T, TU>(Mutator mutator = null) { throw new NotImplementedException(); }
        public override long Count<T>(Mutator mutator = null) { throw new NotImplementedException(); }
        public override T Insert<T>(T model, Mutator mutator = null) { throw new NotImplementedException(); }
        public override T Save<T>(T model, Mutator mutator = null) { throw new NotImplementedException(); }
        public override T Upsert<T>(T model, Mutator mutator = null) { throw new NotImplementedException(); }
        public override void Remove<T>(string key, Mutator mutator = null) { throw new NotImplementedException(); }
        public override void Remove<T>(T model, Mutator mutator = null) { throw new NotImplementedException(); }
        public override void RemoveAll<T>(Mutator mutator = null) { throw new NotImplementedException(); }
        public override IEnumerable<T> BulkInsert<T>(IEnumerable<T> models, Mutator mutator = null) { throw new NotImplementedException(); }
        public override IEnumerable<T> BulkSave<T>(IEnumerable<T> models, Mutator mutator = null) { throw new NotImplementedException(); }
        public override IEnumerable<T> BulkUpsert<T>(IEnumerable<T> models, Mutator mutator = null) { throw new NotImplementedException(); }
        public override void BulkRemove<T>(IEnumerable<string> keys, Mutator mutator = null) { throw new NotImplementedException(); }
        public override void BulkRemove<T>(IEnumerable<T> models, Mutator mutator = null) { throw new NotImplementedException(); }
        public override IEnumerable<T> Where<T>(Expression<Func<T, bool>> predicate, Mutator mutator = null) { throw new NotImplementedException(); }

        #endregion
    }
}