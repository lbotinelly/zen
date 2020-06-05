using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using MySql.Data.MySqlClient;
using Zen.Base;
using Zen.Base.Extension;
using Zen.Base.Module;
using Zen.Base.Module.Data;
using Zen.Module.Data.MySql.Statement;
using Zen.Module.Data.Relational;
using Zen.Module.Data.Relational.Common;
using Zen.Pebble.Database.Common;

namespace Zen.Module.Data.MySql
{
    public class MySqlAdapter<T> : RelationalAdapter<T, MySqlStatementFragments, MySqlWherePart> where T : Data<T>
    {
        private Configuration.IOptions _options;
        private bool _useAdmin;

        #region Overrides of DataAdapterPrimitive<T>

        public override void Setup(Settings<T> settings)
        {
            _options = new Configuration.Options().GetSettings<Configuration.IOptions, Configuration.Options>("Database:MySQL");
        }

        #endregion

        #region Overrides of RelationalAdapter

        public override StatementMasks Masks { get; } = new StatementMasks
        {
            Parameter = "@p{0}",
            Values = { True = 1, False = 0 },
            DefaultTextSize = 255,
            MaximumTextSize = 65535,
            TextOverflowType = "TEXT",
            EnumType = "INT",
            FieldDelimiter = '`',
            TypeMap = new Dictionary<Type, StatementMasks.TypeMapEntry>
            {
                {typeof(int), new StatementMasks.TypeMapEntry("INT")},
                {typeof(long), new StatementMasks.TypeMapEntry("BIGINT")},
                {typeof(bool), new StatementMasks.TypeMapEntry("BIT(1)", "0")},
                {typeof(DateTime), new StatementMasks.TypeMapEntry("TIMESTAMP")},
                {typeof(object), new StatementMasks.TypeMapEntry("BLOB")}
            }
        };

        public override DbConnection GetConnection() => new MySqlConnection(_useAdmin ? _options.AdministrativeConnectionString : Settings.ConnectionString);

        public override void RenderSchemaEntityNames()
        {
            var collectionName = Settings.StorageCollectionName;
            if (collectionName == null) return;

            var tableName = Configuration.SetName ?? collectionName + Masks.Markers.Spacer + Settings.TypeNamespace.ToGuid().ToShortGuid();
            Settings.StorageCollectionName = tableName;
            Settings.ConnectionString ??= _options.ConnectionString;

            var keyField = Settings.Members[Settings.KeyMemberName].TargetName;

            var res = new Dictionary<string, Dictionary<string, KeyValuePair<string, string>>>
            {
                {
                    Categories.Table,
                    new Dictionary<string, KeyValuePair<string, string>>() {{Keys.Schema, new KeyValuePair<string,string>(Keys.Name, tableName)} }

                },
                {
                    Categories.Trigger,
                    new Dictionary<string, KeyValuePair<string, string>>() {{Keys.Schema, new KeyValuePair<string,string>(tableName + "_BI",
                        $"CREATE TRIGGER `{tableName}_BI` BEFORE INSERT ON `{tableName}` FOR EACH ROW begin SET new.{keyField} = uuid(); end") } }

                },
            };

            SchemaElements = res;
        }

        public override void ValidateSchema()
        {
            if (Info<T>.Configuration.IsReadOnly) return;

            //First step - check if the table is there.
            try
            {
                var tableName = SchemaElements[Categories.Table][Keys.Schema].Value;

                try
                {
                    var tableCount = QuerySingleValue<int>("SELECT COUNT(*) FROM information_schema.tables WHERE table_name = '" + tableName + "'");
                    if (tableCount != 0) return;
                }
                catch (MySqlException e)
                {
                    if (!_options.AttempSchemaSetup) throw;

                    // if (e.Message.Contains("is not allowed") || e.Message.Contains("denied")) // Let's try Admin mode.
                    try
                    {
                        _useAdmin = true;
                        // Let's create a default database and user.
                        Execute("CREATE DATABASE IF NOT EXISTS Zen");
                        Execute("SET GLOBAL log_bin_trust_function_creators = 1");
                        Execute($"CREATE USER IF NOT EXISTS 'zenMaster'@'localhost' IDENTIFIED BY 'ExtremelyWeakPassword'");
                        Execute($"GRANT ALL PRIVILEGES ON Zen.* TO 'zenMaster'@'localhost' WITH GRANT OPTION");
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

                        var isNullable = false;

                        //Check if it's a nullable type.

                        var nullProbe = Nullable.GetUnderlyingType(pType);

                        if (nullProbe != null)
                        {
                            isNullable = true;
                            pType = nullProbe;
                        }

                        var (key, value) = Masks.TypeMap.FirstOrDefault(i => pType == i.Key);

                        pDestinyType = key != null ? (value.Name + (value.DefaultValue != null ? " DEFAULT " + value.DefaultValue : "")).Trim() : defaultDestinyType;

                        if (size > Masks.MaximumTextSize) pDestinyType = Masks.TextOverflowType;
                        if (pType.IsEnum) pDestinyType = Masks.EnumType;

                        if (pType == typeof(string)) isNullable = true;

                        //Rendering

                        if (!isNullable) pNullableSpec = " NOT NULL";
                    }
                    else
                    {
                        pDestinyType = Masks.TextOverflowType;
                        pNullableSpec = "";
                    }

                    if (!isFirst) tableRender.Append(", " + Environment.NewLine);
                    else isFirst = false;

                    tableRender.Append($"{Masks.FieldDelimiter}{pSourceName}{Masks.FieldDelimiter} {pDestinyType}{pNullableSpec}");
                }

                // Finally the PK.

                tableRender.AppendLine($"{Environment.NewLine}, PRIMARY KEY(`{Settings.Members[Settings.KeyMemberName].TargetName}`)");

                tableRender.AppendLine(");");

                var rendered = tableRender.ToString();

                Current.Log.Add("Creating table " + tableName);
                Execute(rendered);

                foreach (var (name, creationStatement) in SchemaElements[Categories.Trigger].Values)
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