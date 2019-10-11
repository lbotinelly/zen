using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Reflection;
using System.Text;
using Oracle.ManagedDataAccess.Client;
using Zen.Base;
using Zen.Base.Extension;
using Zen.Base.Module.Data;
using Zen.Module.Data.Relational;
using Zen.Module.Data.Relational.Builder;

namespace Zen.Module.Data.Oracle
{
    public class OracleAdapter : RelationalAdapter
    {
        #region Overrides of RelationalAdapter

        public override DbConnection GetConnection<T>()
        {
            var cn = Info<T>.Settings.ConnectionString;
            return new OracleConnection(cn);
        }

        public override void Initialize<T>()
        {
            Masks = new StatementMasks
            {
                Column = "{0}",
                InlineParameter = ":{0}",
                Parameter = "u_{0}",
                Values =
                {
                    True = 1,
                    False = 0
                }
            };

            StatementBuilder = new StatementBuilder {Masks = Masks};

            Map<T>();
            RenderSchemaEntityNames<T>();
            ValidateSchema<T>();
            PrepareCachedStatements<T>();

            ReferenceCollectionName = Info<T>.Configuration.SetPrefix + Info<T>.Configuration.SetName;
        }

        public override void DropSet<T>(string setName) { throw new NotImplementedException(); }
        public override void CopySet<T>(string sourceSetIdentifier, string targetSetIdentifier, bool flushDestination = false) { throw new NotImplementedException(); }

        public override void RenderSchemaEntityNames<T>()
        {
            var tn = Info<T>.Settings.StorageName;

            if (tn == null) return;

            var trigBaseName = "TRG_" + tn.Replace("TBL_", "");
            if (trigBaseName.Length > 27) trigBaseName = trigBaseName.Substring(0, 27); // Oracle Schema object naming limitation

            var res = new Dictionary<string, KeyValuePair<string, string>>
            {
                {"Sequence", new KeyValuePair<string, string>("SEQUENCE", "SEQ_" + tn.Replace("TBL_", ""))},
                {"Table", new KeyValuePair<string, string>("TABLE", tn)},
                {
                    "BeforeInsertTrigger",
                    new KeyValuePair<string, string>("TRIGGER", trigBaseName + "_BI")
                },
                {
                    "BeforeUpdateTrigger",
                    new KeyValuePair<string, string>("TRIGGER", trigBaseName + "_BU")
                }
            };

            SchemaElements = res;
        }

        public override void ValidateSchema<T>()
        {
            if (Info<T>.Configuration.IsReadOnly) return;

            //First step - check if the table is there.
            try
            {
                var tn = SchemaElements["Table"].Value;

                var tableCount = QuerySingleValue<T, int>("SELECT COUNT(*) FROM ALL_TABLES WHERE table_name = '" + tn + "'");

                if (tableCount != 0) return;

                Current.Log.Add<T>("Initializing schema");

                //Create sequence.
                var seqName = SchemaElements["Sequence"].Value;

                if (seqName.Length > 30) seqName = seqName.Substring(0, 30);

                var tableRender = new StringBuilder();

                tableRender.Append("CREATE TABLE " + tn + "(");

                var isFirst = true;

                //bool isRCTSFound = false;
                //bool isRUTSFound = false;

                foreach (var prop in typeof(T).GetProperties())
                {
                    var pType = prop.PropertyType;

                    var pSourceName = prop.Name;

                    long size = 255;

                    if (MemberDescriptors.ContainsKey(pSourceName))
                    {
                        size = MemberDescriptors[pSourceName].Length;
                        if (size == 0) size = 255;
                    }

                    var pDestinyType = "VARCHAR2(" + size + ")";
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

                        if (pType == typeof(long)) pDestinyType = "NUMBER (20)";
                        if (pType == typeof(int)) pDestinyType = "NUMBER (20)";
                        if (pType == typeof(DateTime)) pDestinyType = "TIMESTAMP";
                        if (pType == typeof(bool)) pDestinyType = "NUMBER (1) DEFAULT 0";
                        if (pType == typeof(object)) pDestinyType = "BLOB";
                        if (size > 4000) pDestinyType = "BLOB";
                        if (pType.IsEnum) pDestinyType = "NUMBER (10)";

                        if (pType == typeof(string)) isNullable = true;

                        if (MemberDescriptors.ContainsKey(pSourceName)) pSourceName = MemberDescriptors[pSourceName].Field;

                        var bMustSkip =
                            pSourceName.ToLower().Equals("rcts") ||
                            pSourceName.ToLower().Equals("ruts");

                        if (bMustSkip) continue;

                        if (string.Equals(pSourceName, KeyColumn,
                                          StringComparison.CurrentCultureIgnoreCase)) isNullable = false;

                        if (string.Equals(pSourceName, KeyMember,
                                          StringComparison.CurrentCultureIgnoreCase)) isNullable = false;

                        //Rendering

                        if (!isNullable) pNullableSpec = " NOT NULL";
                    }
                    else
                    {
                        pDestinyType = "CLOB";
                        pNullableSpec = "";
                    }

                    if (!isFirst) tableRender.Append(", ");
                    else isFirst = false;

                    tableRender.Append(pSourceName + " " + pDestinyType + pNullableSpec);
                }

                //if (!isRCTSFound) tableRender.Append(", RCTS TIMESTAMP  DEFAULT CURRENT_TIMESTAMP");
                //if (!isRUTSFound) tableRender.Append(", RUTS TIMESTAMP  DEFAULT CURRENT_TIMESTAMP");

                tableRender.Append(", RCTS TIMESTAMP  DEFAULT CURRENT_TIMESTAMP");
                tableRender.Append(", RUTS TIMESTAMP  DEFAULT CURRENT_TIMESTAMP");

                tableRender.Append(")");

                try
                {
                    Current.Log.Add<T>("Creating table " + tn);
                    Execute<T>(tableRender.ToString());
                } catch (Exception e) { Current.Log.Add(e); }

                if (KeyColumn != null)
                {
                    try { Execute<T>("DROP SEQUENCE " + seqName); } catch { }

                    try
                    {
                        Current.Log.Add<T>("Creating Sequence " + seqName);
                        Execute<T>("CREATE SEQUENCE " + seqName);
                    } catch (Exception e) { }

                    //Primary Key
                    var pkName = tn + "_PK";
                    var pkStat = $"ALTER TABLE {tn} ADD (CONSTRAINT {pkName} PRIMARY KEY ({KeyColumn}))";

                    try
                    {
                        Current.Log.Add<T>("Adding Primary Key constraint " + pkName + " (" + KeyColumn + ")");
                        Execute<T>(pkStat);
                    } catch (Exception e) { Current.Log.Add(e); }
                }
                //Trigger

                var trigStat =
                    @"CREATE OR REPLACE TRIGGER {0}
                BEFORE INSERT ON {1}
                FOR EACH ROW
                BEGIN
                " +
                    (KeyColumn != null
                        ? @"IF :new.{3} is null 
                    THEN       
                        SELECT {2}.NEXTVAL INTO :new.{3} FROM dual;
                    END IF;"
                        : "")
                    + @"  
                :new.RCTS := CURRENT_TIMESTAMP;
                :new.RUTS := CURRENT_TIMESTAMP;
                END;";

                try
                {
                    Current.Log.Add<T>("Adding BI Trigger " + SchemaElements["BeforeInsertTrigger"].Value);
                    Execute<T>(
                        string.Format(trigStat,
                                      SchemaElements["BeforeInsertTrigger"].Value, tn, seqName,
                                      KeyColumn));
                } catch (Exception e) { Current.Log.Add(e); }

                trigStat =
                    @"CREATE OR REPLACE TRIGGER {0}
                BEFORE UPDATE ON {1}
                FOR EACH ROW
                BEGIN
                :new.RUTS := CURRENT_TIMESTAMP;
                END;";

                try
                {
                    Current.Log.Add<T>("Adding BU Trigger " + SchemaElements["BeforeUpdateTrigger"].Value);

                    Execute<T>(string.Format(trigStat,
                                             SchemaElements["BeforeUpdateTrigger"].Value, tn, seqName,
                                             KeyColumn));
                } catch (Exception e) { Current.Log.Add(e); }

                //Now, add comments to everything.

                var ocld = " - ";
                var ocfld = ";";
                var commentStat =
                    "COMMENT ON TABLE " + tn + " IS 'Auto-generated table for Entity " + typeof(T).FullName + ". " +
                    "Supporting structures - " +
                    " Sequence: " + seqName + "; " +
                    " Triggers: " +
                    SchemaElements["BeforeInsertTrigger"].Value
                    + ", " +
                    SchemaElements["BeforeUpdateTrigger"].Value
                    + ".'" + ocfld;

                try { Execute<T>(commentStat); } catch { }

                //'Event' hook for post-schema initialization procedure:
                try
                {
                    typeof(T).GetMethod("OnSchemaInitialization", BindingFlags.Public | BindingFlags.Static)
                        .Invoke(null, null);
                } catch { }
            } catch (Exception e)
            {
                Current.Log.Warn<T>("Schema render Error: " + e.Message);
                Current.Log.Add(e);
                throw;
            }
        }

        #endregion
    }
}