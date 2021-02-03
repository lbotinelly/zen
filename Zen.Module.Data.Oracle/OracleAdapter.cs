using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Text;
using Oracle.ManagedDataAccess.Client;
using Zen.Base;
using Zen.Base.Extension;
using Zen.Base.Module;
using Zen.Base.Module.Data;
using Zen.Module.Data.Oracle.Statement;
using Zen.Module.Data.Relational;
using Zen.Module.Data.Relational.Common;
using Zen.Pebble.Database.Common;

namespace Zen.Module.Data.Oracle
{
    public class OracleAdapter<T> : RelationalAdapter<T, OracleStatementFragments, OracleWherePart> where T : Data<T>
    {
        private Configuration.IOptions _options;
        private bool _useAdmin;

        public override StatementMasks Masks { get; } = new StatementMasks
        {
            ParameterPrefix = ":u_",
            BooleanValues = { True = 1, False = 0 },
            DefaultTextSize = 255,
            MaximumTextSize = 65535,
            TextOverflowType = "NVARCHAR",
            EnumType = "INT",
            LeftFieldDelimiter = '[',
            RightFieldDelimiter = ']'
        };

        public override Dictionary<Type, TypeDefinition> TypeDefinitionMap { get; } = new Dictionary<Type, TypeDefinition>
        {
            {
                typeof(int), new TypeDefinition("NUMBER(20)")
                {
                    DiscreteAutoSchema = new List<string>
                    {
                        "CREATE SEQUENCE {StorageCollectionName}_SEQ",
                        @"CREATE TRIGGER {StorageCollectionName}_BI
BEFORE INSERT ON {StorageCollectionName}
FOR EACH ROW
BEGIN
  SELECT {StorageCollectionName}_SEQ.nextval
  INTO :new.{StorageKeyMemberName}
  FROM dual;
END;
"
                    }
                }
            },
            {
                typeof(long), new TypeDefinition("NUMBER(20)")
                {
                    DiscreteAutoSchema = new List<string>
                    {
                        "CREATE SEQUENCE {StorageCollectionName}_SEQ",
                        @"CREATE TRIGGER {StorageCollectionName}_BI
BEFORE INSERT ON {StorageCollectionName}
FOR EACH ROW
BEGIN
  SELECT {StorageCollectionName}_SEQ.nextval
  INTO :new.{StorageKeyMemberName}
  FROM dual;
END;
"
                    }
                }
            },
            {typeof(bool), new TypeDefinition("NUMBER(1)", "0")},
            {typeof(DateTime), new TypeDefinition("TIMESTAMP")},
            {typeof(string), new TypeDefinition("NVARCHAR({DefaultTextSize})") {InlineAutoSchema = "SYS_GUID()"}},
            {typeof(object), new TypeDefinition("BLOB")}
        };

        #region Overrides of RelationalAdapter

        public override DbConnection GetConnection()
        {
            var cn = Info<T>.Settings.ConnectionString;
            return new OracleConnection(cn);
        }

        public override void Setup(Settings<T> settings)
        {
            _options = new Configuration.Options().GetSettings<Configuration.IOptions, Configuration.Options>("Database:Oracle");
            Statements.InsertModel = "INSERT INTO {StorageCollectionName} ({InlineFieldSet}) VALUES ({InlineParameterSet})";
        }

        public override void DropSet(string setName) => throw new NotImplementedException();

        public override void CopySet(string sourceSetIdentifier, string targetSetIdentifier, bool flushDestination = false) => throw new NotImplementedException();

        public override void RenderSchemaEntityNames()
        {
            var collectionName = Settings.StorageCollectionName;
            if (collectionName == null) return;

            var tableName = Configuration.SetName ?? collectionName + Masks.Markers.Spacer + Settings.TypeNamespace.ToGuid().ToShortGuid();
            Settings.StorageCollectionName = tableName;
            Settings.ConnectionString ??= _options.ConnectionString;

            Settings.StorageKeyMemberName = Settings.Members[Settings.KeyMemberName].TargetName;

            var settingsDict = Settings.ToMemberDictionary();

            var res = new Dictionary<string, Dictionary<string, string>>
            {
                {
                    Categories.Table,
                    new Dictionary<string, string> {{Keys.Name, tableName}}
                }
            };

            if (TypeDefinitionMap.ContainsKey(Settings.Members[Settings.KeyMemberName].Type))
            {
                var discreteKeySchema = TypeDefinitionMap[Settings.Members[Settings.KeyMemberName].Type]?.DiscreteAutoSchema;

                if (discreteKeySchema != null)
                    foreach (var keySchema in discreteKeySchema)
                    {
                        var renderedKeySchema = settingsDict.ReplaceIn(keySchema);

                        if (!res.ContainsKey(Categories.Trigger)) res.Add(Categories.Trigger, new Dictionary<string, string>());
                        if (res[Categories.Trigger] == null) res[Categories.Trigger] = new Dictionary<string, string>();

                        res[Categories.Trigger].Add(renderedKeySchema.Md5Hash(), renderedKeySchema);
                    }
            }

            SchemaElements = res;
        }

        public override void ValidateSchema()
        {
            if (Info<T>.Configuration.IsReadOnly) return;

            //First step - check if the table is there.
            try
            {
                var tableName = SchemaElements[Categories.Table][Keys.Name];

                try
                {
                    var tableCount = QuerySingleValue<int>($"SELECT COUNT(*) FROM information_schema.tables WHERE table_name = '{tableName}'");
                    if (tableCount != 0) return;
                }
                catch (Exception e)
                {
                    if (!_options.AttempSchemaSetup) throw;

                    // if (e.Message.Contains("is not allowed") || e.Message.Contains("denied")) // Let's try Admin mode.
                    try
                    {
                        _useAdmin = true;
                        // Let's create a default database and user.
                        Execute("CREATE DATABASE IF NOT EXISTS Zen");
                        Execute("SET GLOBAL log_bin_trust_function_creators = 1");
                        Execute("CREATE USER IF NOT EXISTS 'zenMaster'@'localhost' IDENTIFIED BY 'ExtremelyWeakPassword'");
                        Execute("GRANT ALL PRIVILEGES ON Zen.* TO 'zenMaster'@'localhost' WITH GRANT OPTION");
                        Execute("FLUSH PRIVILEGES");
                        _useAdmin = false;
                    }
                    catch (Exception exception)
                    {
                        _useAdmin = false;
                        Console.WriteLine(exception);
                        throw;
                    }
                }

                Current.Log.Add("Initializing schema");

                var maskKeys = Masks.ToMemberDictionary();

                //Create sequence.
                var tableRender = new StringBuilder();

                tableRender.AppendLine("CREATE TABLE IF NOT EXISTS " + tableName + " (");

                var isFirst = true;

                foreach (var (name, memberDescriptor) in Settings.Members)
                {
                    var pType = memberDescriptor.Type;
                    long size = memberDescriptor.Size ?? Masks.DefaultTextSize;

                    var pSourceName = memberDescriptor.TargetName;

                    var pDestinyType = "";
                    var pAutoSchema = "";
                    var defaultDestinyType = $"VARCHAR ({Masks.DefaultTextSize})";
                    var pNullableSpec = "";

                    if (pType.IsPrimitiveType())
                    {
                        if (pType.IsArray) continue;
                        if (!(typeof(string) == pType) && typeof(IEnumerable).IsAssignableFrom(pType)) continue;
                        if (typeof(ICollection).IsAssignableFrom(pType)) continue;
                        if (typeof(IList).IsAssignableFrom(pType)) continue;
                        if (typeof(IDictionary).IsAssignableFrom(pType)) continue;

                        if (pType.BaseType != null && typeof(IList).IsAssignableFrom(pType.BaseType) && pType.BaseType.IsGenericType) continue;

                        var isNullable = pType.IsNullable();

                        if (Settings.StorageKeyMemberName == memberDescriptor.SourceName) isNullable = false;

                        var (key, value) = TypeDefinitionMap.FirstOrDefault(i => pType == i.Key);

                        if (key != null)
                        {
                            pDestinyType = value.Name;
                            if (value.DefaultValue != null) pDestinyType += " DEFAULT " + value.DefaultValue;

                            if (name == Settings.KeyMemberName)
                                if (value.InlineAutoSchema != null)
                                    pAutoSchema = value.InlineAutoSchema;
                        }
                        else
                        {
                            pDestinyType = defaultDestinyType;
                        }

                        if (size > Masks.MaximumTextSize) pDestinyType = Masks.TextOverflowType;
                        if (pType.IsEnum) pDestinyType = Masks.EnumType;

                        //Rendering

                        if (!isNullable) pNullableSpec = "NOT NULL";
                    }
                    else
                    {
                        pDestinyType = Masks.TextOverflowType;
                        pNullableSpec = "";
                    }

                    if (!isFirst) tableRender.Append(", " + Environment.NewLine);
                    else isFirst = false;

                    tableRender.Append($"{Masks.LeftFieldDelimiter}{pSourceName}{Masks.RightFieldDelimiter} {pDestinyType} {pNullableSpec} {pAutoSchema}");
                }

                // Finally the PK.

                tableRender.AppendLine($"{Environment.NewLine}, PRIMARY KEY(`{Settings.Members[Settings.KeyMemberName].TargetName}`)");

                tableRender.AppendLine(");");

                var rendered = tableRender.ToString();

                rendered = maskKeys.ReplaceIn(rendered);

                Current.Log.Add("Creating table " + tableName);

                Current.Log.Add("---");
                Current.Log.Add(rendered);
                Execute(rendered);
                Current.Log.Add("---");

                foreach (var (name, creationStatement) in SchemaElements[Categories.Trigger])
                {
                    Current.Log.Add($"Creating {Categories.Trigger} {name}");
                    Execute(creationStatement);
                }

                //'Event' hook for post-schema initialization procedure:
                typeof(T).GetMethod("OnSchemaInitialization", BindingFlags.Public | BindingFlags.Static)?.Invoke(null, null);
            }
            catch (Exception e)
            {
                Current.Log.Warn("Schema render Error: " + e.Message);
                Current.Log.Add(e);
                throw;
            }
        }

        #endregion
    }
}