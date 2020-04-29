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
using Zen.Module.Data.Relational.Common;
using Zen.Module.Data.Relational.Mapper;
using Zen.Pebble.Database;
using Zen.Pebble.Database.Common;

namespace Zen.Module.Data.Relational
{
    public abstract class RelationalAdapter<T, TStatementFragments, TWherePart> : 
        DataAdapterPrimitive<T>, IRelationalStatements 
        where T : Data<T> 
        where TStatementFragments : IStatementFragments 
        where TWherePart : IWherePart
    {
        public Dictionary<string, string> MemberMap = new Dictionary<string, string>();
        public abstract StatementMasks Masks { get; }

        public void Map()
        {
            var cat = new ColumnAttributeTypeMapper<T>();
            SqlMapper.SetTypeMap(typeof(T), cat);

            MemberDescriptors =
                (from pInfo in typeof(T).GetProperties()
                    let p1 = pInfo.GetCustomAttributes(false).OfType<ColumnAttribute>().ToList()
                    let field = p1.Count != 0 ? p1[0].Name ?? pInfo.Name : pInfo.Name
                    let length = p1.Count != 0 ? p1[0].Length != 0 ? p1[0].Length : 0 : 0
                    let serializable = p1.Count != 0 && p1[0].Serialized
                    select new KeyValuePair<string, MemberDescriptor>(pInfo.Name, new MemberDescriptor {Field = field, Length = length, Serializable = serializable})
                ).ToDictionary(x => x.Key, x => x.Value);

            MemberMap = MemberDescriptors.Select(i => new KeyValuePair<string, string>(i.Key, i.Value.Field)).ToDictionary(i => i.Key, i => i.Value);

            var (key, value) = MemberDescriptors.FirstOrDefault(p => p.Value.Field.ToLower().Equals(Info<T>.Settings.KeyMemberName.ToLower()));

            KeyMember = key;
            KeyColumn = value.Field;
        }

        public virtual void PrepareStatements()
        {
            var setName = Configuration.SetName;

            // "SELECT COUNT(*) FROM {0}"
            Statements.RowCount = Statements.RowCount.format(setName);

            // "SELECT * FROM {0} WHERE {1} = {2}"
            Statements.GetSingleByIdentifier = Statements.GetSingleByIdentifier.format(setName, KeyColumn, Masks.InlineParameter.format(Masks.Parameter.format(KeyColumn)));

            // "SELECT * FROM {0} WHERE {1} IN ({2})"
            Statements.GetManyByIdentifier = Statements.GetManyByIdentifier.format(setName, KeyColumn, Masks.InlineParameter.format(Masks.Parameter.format(Masks.Keywords.Keyset)));

            // "SELECT * FROM {0}"
            Statements.GetAll = Statements.GetAll.format(setName);

            // "SELECT * FROM {0} WHERE {1}"
            Statements.AllFields = Statements.AllFields.format(setName, "{0}");
        }

        #region Custom Members

        public T1 QuerySingleValue<T1>(string statement)
        {
            using (var conn = GetConnection())
            {
                var ret = conn.Query<T1>(statement).FirstOrDefault();
                return ret;
            }
        }

        public void Execute(string statement)
        {
            using (var conn = GetConnection())
            {
                conn.Execute(statement);
            }
        }

        public List<TU> RawQuery<TU>(string statement, object parameters)
        {
            try
            {
                using (var conn = GetConnection())
                {
                    conn.Open();

                    var o = conn.Query(statement, parameters)
                        .Select(a => (IDictionary<string, object>) a)
                        .ToList();
                    conn.Close();

                    var ret = o.Select(refObj => refObj.GetObject<TU>(MemberMap)).ToList();

                    return ret;
                }
            }
            catch (Exception e)
            {
                throw new DataException(Info<T>.Settings.TypeQualifiedName + " RelationalAdapter: Error while issuing statements to the database.", e);
            }
        }

        public List<TU> AdapterQuery<TU>(string statement, Mutator mutator = null)
        {
            if (mutator == null) mutator = new Mutator {Transform = new QueryTransform {Statement = statement}};

            var builder = mutator.ToSqlBuilderTemplate();

            return RawQuery<TU>(builder.RawSql, builder.Parameters);
        }

        #endregion

        #region Implementation of IRelationalStatements

        public virtual bool UseIndependentStatementsForKeyExtraction { get; } = false;
        public virtual bool UseNumericPrimaryKeyOnly { get; } = false;
        public virtual bool UseOutputParameterForInsertedKeyExtraction { get; } = false;
        public virtual RelationalStatements Statements { get; } = new RelationalStatements();
        public Dictionary<string, MemberDescriptor> MemberDescriptors { get; set; } = new Dictionary<string, MemberDescriptor>();

        public string KeyMember { get; set; }
        public string KeyColumn { get; set; }
        public Dictionary<string, KeyValuePair<string, string>> SchemaElements { get; set; }

        public virtual DbConnection GetConnection() => null;
        public virtual void RenderSchemaEntityNames() { }
        public virtual void ValidateSchema() { }

        #endregion

        #region Overrides of DataAdapterPrimitive


        public override void Initialize()
        {
            StatementRender = new StatementRender<T, TStatementFragments, TWherePart> {Masks = Masks};
            Configuration = Info<T>.Configuration;
            Settings = Info<T>.Settings;

            Map();
            RenderSchemaEntityNames();
            ValidateSchema();
            PrepareStatements();

            ReferenceCollectionName = Configuration.SetPrefix + Configuration.SetName;
        }

        public StatementRender<T, TStatementFragments, TWherePart> StatementRender;

        public Settings<T> Settings { get; private set; }

        public DataConfigAttribute Configuration { get; private set; }

        public override long Count(Mutator mutator = null) => QuerySingleValue<long>(Statements.RowCount);
        public override T Insert(T model, Mutator mutator = null) => throw new NotImplementedException();
        public override T Save(T model, Mutator mutator = null) => throw new NotImplementedException();
        public override T Upsert(T model, Mutator mutator = null) => throw new NotImplementedException();
        public override void Remove(string key, Mutator mutator = null)
        {
            throw new NotImplementedException();
        }
        public override void Remove(T model, Mutator mutator = null)
        {
            throw new NotImplementedException();
        }
        public override void RemoveAll(Mutator mutator = null)
        {
            throw new NotImplementedException();
        }
        public override IEnumerable<T> BulkInsert(IEnumerable<T> models, Mutator mutator = null) => throw new NotImplementedException();
        public override IEnumerable<T> BulkSave(IEnumerable<T> models, Mutator mutator = null) => throw new NotImplementedException();
        public override IEnumerable<T> BulkUpsert(IEnumerable<T> models, Mutator mutator = null) => throw new NotImplementedException();
        public override void BulkRemove(IEnumerable<string> keys, Mutator mutator = null)
        {
            throw new NotImplementedException();
        }
        public override void BulkRemove(IEnumerable<T> models, Mutator mutator = null)
        {
            throw new NotImplementedException();
        }

        public override void DropSet(string setName)
        {
            throw new NotImplementedException();
        }

        public override void CopySet(string sourceSetIdentifier, string targetSetIdentifier, bool flushDestination = false)
        {
            throw new NotImplementedException();
        }

        public override T Get(string key, Mutator mutator = null)
        {
            var statement = Statements.GetSingleByIdentifier;
            var parameter = new Dictionary<string, object> {{Masks.Parameter.format(KeyColumn), key}};

            return RawQuery<T>(statement, parameter).FirstOrDefault();
        }

        public override IEnumerable<T> Get(IEnumerable<string> keys, Mutator mutator = null)
        {
            var keySet = keys as string[] ?? keys.ToArray();

            if (!keySet.Any()) return new List<T>();

            var statement = Statements.GetManyByIdentifier;
            var parameter = new Dictionary<string, object> {{Masks.Parameter.format(Masks.Keywords.Keyset), keySet.ToArray()}};
            return RawQuery<T>(statement, parameter);
        }

        public override IEnumerable<T> Query(string statement) => throw new NotImplementedException();

        public override IEnumerable<T> Query(Mutator mutator = null) => Query<T>(Statements.GetAll);
        public override IEnumerable<TU> Query<TU>(string statement) => throw new NotImplementedException();

        public override IEnumerable<TU> Query<TU>(Mutator mutator = null) => AdapterQuery<TU>(Statements.GetAll, mutator);

        public override IEnumerable<T> Where(Expression<Func<T, bool>> predicate, Mutator mutator = null)
        {
            var parts = StatementRender.Render(predicate);
            var statement = Statements.AllFields.format(parts.Statement);

            return RawQuery<T>(statement, parts.Parameters);
        }

        #endregion
    }
}