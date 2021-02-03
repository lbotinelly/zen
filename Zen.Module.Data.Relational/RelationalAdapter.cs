using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using Dapper;
using Zen.Base;
using Zen.Base.Extension;
using Zen.Base.Module;
using Zen.Base.Module.Data;
using Zen.Base.Module.Data.Adapter;
using Zen.Base.Module.Log;
using Zen.Module.Data.Relational.Mapper;
using Zen.Pebble.Database;
using Zen.Pebble.Database.Common;

namespace Zen.Module.Data.Relational
{
    public abstract class RelationalAdapter<T, TStatementFragments, TWherePart> : DataAdapterPrimitive<T>, IRelationalStatements
        where T : Data<T>
        where TStatementFragments : IStatementFragments
        where TWherePart : IWherePart
    {
        public Dictionary<string, string> MemberMap = new Dictionary<string, string>();
        public abstract StatementMasks Masks { get; }
        public abstract Dictionary<Type, TypeDefinition> TypeDefinitionMap { get; }
        public IDictionary<string, object> ToRawParameters(object source) => ToRawParameters(new Dictionary<string, object>(source.ToMemberDictionary(Masks.ParameterPrefix)));
        public IDictionary<string, object> ToRawParameters(Dictionary<string, object> probe)
        {
            var res = new Dictionary<string, object>();

            foreach (var (key, value) in probe)
            {
                res[key] = value;

                if (Masks.BooleanValues != null)
                    if (value is bool)
                        res[key] = (bool)value ? Masks.BooleanValues.True : Masks.BooleanValues.False;

                if (value is Enum) res[key] = (int)value;
            }

            return res;
        }

        public void Map()
        {
            SqlMapper.SetTypeMap(typeof(T), new ColumnAttributeTypeMapper<T>());

            // For each of the custom type members, map it as a Json object:

            foreach (var memberAttribute in Settings.Members.Where(memberAttribute => !memberAttribute.Value.Type.IsBasicType())) SqlMapper.AddTypeHandler(memberAttribute.Value.Type, new JsonObjectTypeHandler());
        }

        [SuppressMessage("ReSharper", "InconsistentNaming")]
        public virtual void PrepareStatements()
        {
            var membersSansKey = Settings.Members.Where(i => i.Key != Settings.KeyMemberName).ToList();

            var InlineFieldSet = string.Join(", ", Settings.Members.Select(i => i.Value.TargetName));
            var InlineParameterSet = string.Join(", ", Settings.Members.Select(i => Masks.ParameterPrefix + i.Value.SourceName));
            var InterspersedFieldParameterSet = string.Join(", ", Settings.Members.Select(i => i.Value.TargetName + Masks.Markers.Assign + Masks.ParameterPrefix + i.Value.SourceName));

            var InlineFieldSetSansKey = string.Join(", ", membersSansKey.Select(i => i.Value.TargetName));
            var InlineParameterSetSansKey = string.Join(", ", membersSansKey.Select(i => Masks.ParameterPrefix + i.Value.SourceName));
            var InterspersedFieldParameterSetSansKey = string.Join(", ", membersSansKey.Select(i => i.Value.TargetName + Masks.Markers.Assign + Masks.ParameterPrefix + i.Value.SourceName));

            var preCol = new
            {
                Settings.StorageCollectionName,
                Settings.StorageKeyMemberName,
                Masks.ParameterPrefix,
                Settings.KeyMemberName,
                Masks.SetParameterName,
                InlineFieldSet,
                InlineParameterSet,
                InterspersedFieldParameterSet,
                InlineFieldSetSansKey,
                InlineParameterSetSansKey,
                InterspersedFieldParameterSetSansKey
            }.ToMemberDictionary();

            Statements.DropSet = preCol.ReplaceIn(Statements.DropSet);

            Statements.RowCount = preCol.ReplaceIn(Statements.RowCount);
            Statements.CheckKey = preCol.ReplaceIn(Statements.CheckKey);

            Statements.ParametrizedKeyField = preCol.ReplaceIn(Statements.ParametrizedKeyField);

            Statements.InsertModel = preCol.ReplaceIn(Statements.InsertModel);
            Statements.UpdateModel = preCol.ReplaceIn(Statements.UpdateModel);
            Statements.RemoveModel = preCol.ReplaceIn(Statements.RemoveModel);

            Statements.GetModelByIdentifier = preCol.ReplaceIn(Statements.GetModelByIdentifier);
            Statements.GetSetByIdentifiers = preCol.ReplaceIn(Statements.GetSetByIdentifiers);
            Statements.RemoveSetByIdentifiers = preCol.ReplaceIn(Statements.RemoveSetByIdentifiers);

            Statements.GetSetByWhere = preCol.ReplaceIn(Statements.GetSetByWhere);
            Statements.GetSetComplete = preCol.ReplaceIn(Statements.GetSetComplete);
        }

        public class TypeDefinition
        {
            public string DefaultValue;
            public List<string> DiscreteAutoSchema;
            public string InlineAutoSchema;
            public string Name;

            public TypeDefinition(string name, string defaultValue = null)
            {
                Name = name;
                DefaultValue = defaultValue;
            }
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

        public void Execute(string statement, object parameters = null)
        {
            Current.Log.Add("Execute: " + statement, Message.EContentType.Debug);

            if (parameters != null) Current.Log.Add(parameters.ToJson(), Message.EContentType.Debug);

            using (var conn = GetConnection()) { conn.Execute(statement, parameters); }
        }

        public TE ExecuteScalar<TE>(string statement, object parameters = null)
        {
            Current.Log.Add("ExecuteScalar: " + statement, Message.EContentType.Debug);

            if (parameters != null) Current.Log.Add(parameters.ToJson(), Message.EContentType.Debug);

            using (var conn = GetConnection()) { return (TE)conn.ExecuteScalar(statement, parameters); }
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
            } catch (Exception e)
            {
                Current.Log.Add(statement, Message.EContentType.Exception);
                if (parameters != null) Current.Log.Add(parameters.ToJson(), Message.EContentType.Exception);

                throw new DataException(Info<T>.Settings.TypeQualifiedName + " RelationalAdapter: Error while issuing statements to the database.", e);
            }
        }
        public List<TU> AdapterQuery<TU>(string statement, Mutator mutator = null)
        {
            mutator = mutator.SetStatement(statement);
            var builder = mutator.ToSqlBuilderTemplate(Settings, Masks);
            var sql = builder.RawSql;

            if (mutator.Transform?.Pagination?.Size > 0) sql = AddPaginationWrapper(sql, mutator.Transform.Pagination);

            return RawQuery<TU>(sql, builder.Parameters);
        }
        public virtual string AddPaginationWrapper(string sql, Pagination pagination)
        {
            var parameters = new
            {
                pagination.Size,
                pagination.Index,
                StartPosition = pagination.Index * pagination.Size,
                EndPosition = (pagination.Index + 1) * pagination.Size - 1,
                OriginalQuery = sql
            }.ToMemberDictionary();

            return parameters.ReplaceIn(Statements.PaginationWrapper);
        }
        #endregion

        #region Implementation of IRelationalStatements
        public virtual bool UseIndependentStatementsForKeyExtraction { get; } = false;
        public virtual bool UseNumericPrimaryKeyOnly { get; } = false;
        public virtual bool UseOutputParameterForInsertedKeyExtraction { get; } = false;
        public virtual RelationalStatements Statements { get; } = new RelationalStatements();
        public string KeyMember { get; set; }
        public string KeyColumn { get; set; }
        public Dictionary<string, Dictionary<string, string>> SchemaElements { get; set; }
        public virtual DbConnection GetConnection() { return null; }
        public virtual void RenderSchemaEntityNames() { }
        public virtual void ValidateSchema() { }
        #endregion

        #region Overrides of DataAdapterPrimitive
        public override void Initialize()
        {
            Configuration = Info<T>.Configuration;
            Settings = Info<T>.Settings;

            Map();
            RenderSchemaEntityNames();
            ValidateSchema();

            KeyColumn = Settings.Members[Settings.KeyMemberName].TargetName;

            ModelRender = new ModelRender<T, TStatementFragments, TWherePart>(Extensions.ToModelDescriptor<T>(), Masks);

            PrepareStatements();

            ReferenceCollectionName = Configuration.SetPrefix + Configuration.SetName;
        }
        public ModelRender<T, TStatementFragments, TWherePart> ModelRender { get; set; }
        public Settings<T> Settings { get; private set; }
        public DataConfigAttribute Configuration { get; private set; }
        public override long Count(Mutator mutator = null) => QuerySingleValue<long>(Statements.RowCount);
        public override T Insert(T model, Mutator mutator = null)
        {
            if (model == null) return null;
            Execute(Statements.InsertModel, ToRawParameters(model));
            return Get(model.GetDataKey(), mutator);
        }
        public override T Save(T model, Mutator mutator = null)
        {
            if (model == null) return null;
            Execute(Statements.UpdateModel, ToRawParameters(model));
            return Get(model.GetDataKey(), mutator);
        }
        public override bool KeyExists(string key, Mutator mutator = null) => ExecuteScalar<long>(Statements.CheckKey, KeyParameter(key)) > 0;
        // ReSharper disable once IdentifierTypo
        public override T Upsert(T model, Mutator mutator = null) => !KeyExists(model.GetDataKey()) ? Insert(model, mutator) : Save(model, mutator);
        public override void Remove(string key, Mutator mutator = null) => Execute(Statements.RemoveModel, KeyParameter(key));
        public override void Remove(T model, Mutator mutator = null) => Remove(model.GetDataKey(), mutator);
        public override void RemoveAll(Mutator mutator = null) => Execute(Statements.DropSet);
        public override IEnumerable<T> BulkInsert(IEnumerable<T> models, Mutator mutator = null) => BulkUpsert(models, mutator);
        public override IEnumerable<T> BulkSave(IEnumerable<T> models, Mutator mutator = null) => BulkUpsert(models, mutator);

        public override IEnumerable<T> BulkUpsert(IEnumerable<T> models, Mutator mutator = null)
        {
            models = models.ToList();
            foreach (var model in models) model.Save();
            return models;
        }
        public override void BulkRemove(IEnumerable<string> keys, Mutator mutator = null)
        {
            var keySet = keys as string[] ?? keys.ToArray();
            if (!keySet.Any()) return;

            var statement = Statements.RemoveSetByIdentifiers;
            var parameter = new Dictionary<string, object> { { Statements.ParametrizedKeyField, keySet.ToArray() } };

            Execute(statement, parameter);
        }
        public override void BulkRemove(IEnumerable<T> models, Mutator mutator = null) => BulkRemove(models.Select(i => i.GetDataKey()).ToList(), mutator);
        public override void DropSet(string setName) => throw new NotSupportedException("Set operations not supported by Relational adapters");
        public override void CopySet(string sourceSetIdentifier, string targetSetIdentifier, bool flushDestination = false) => throw new NotSupportedException("Set operations not supported by Relational adapters");
        public override T Get(string key, Mutator mutator = null) => RawQuery<T>(Statements.GetModelByIdentifier, KeyParameter(key)).FirstOrDefault();
        private Dictionary<string, object> KeyParameter(string key) => new Dictionary<string, object> { { Statements.ParametrizedKeyField, key } };

        public override IEnumerable<T> Get(IEnumerable<string> keys, Mutator mutator = null)
        {
            var keySet = keys as string[] ?? keys.ToArray();

            if (!keySet.Any()) return new List<T>();

            var statement = Statements.GetSetByIdentifiers;
            var parameter = new Dictionary<string, object> { { Statements.ParametrizedKeyField, keySet.ToArray() } };

            return RawQuery<T>(statement, parameter);
        }
        public override IEnumerable<T> Query(string statement) => RawQuery<T>(statement, null);
        public override IEnumerable<T> Query(Mutator mutator = null) => Query<T>(Statements.GetSetComplete);
        public override IEnumerable<TU> Query<TU>(string statement) => AdapterQuery<TU>(Statements.GetSetComplete);
        public override IEnumerable<TU> Query<TU>(Mutator mutator = null) => AdapterQuery<TU>(Statements.GetSetComplete, mutator);
        public override IEnumerable<T> Where(Expression<Func<T, bool>> predicate, Mutator mutator = null)
        {
            var parts = ModelRender.Render(predicate);
            var statement = Statements.GetSetByWhere.format(parts.Statement);
            var parameters = ToRawParameters(ModelRender.Render(predicate).Parameters);

            return RawQuery<T>(statement, parameters);
        }
        #endregion
    }
}