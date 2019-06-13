using System.Data;
using System.Data.Common;
using Zen.Base.Module;
using Zen.Base.Module.Data.Adapter;

namespace Zen.Module.Data.MongoDB
{
    public class StaticLock<T> where T : Data<T>
    {
        public static object Lock { get; set; } = new object();
    }

    public class MongoDbAdapter : DataAdapterPrimitive
    {
        public MongoDbAdapter()
        {
            useOutputParameterForInsertedKeyExtraction = true;
            dynamicParameterType = typeof(MongoDbDynamicParameters);
            Interceptor = new MongoDbinterceptor(this);
        }

        public override void RenderSchemaEntityNames<T>() { }
        public override DbConnection Connection(string connectionString) { return null; }
        public override void CheckDatabaseEntities<T>() { }
    }

    public class MongoDbDynamicParameters : DynamicParametersPrimitive
    {
        public MongoDbDynamicParameters()
        {
            // CommandType = typeof(OracleCommand);
            // ParameterType = typeof(OracleParameter);

            ParameterDefinition.Identifier = ":";
        }

        public override void Add(string name, object value = null, DbGenericType? dbType = null, ParameterDirection? direction = null, int? size = null)
        {
            if (value is bool) value = (bool) value ? 1 : 0; //Oracle doesn't like BOOL.

            base.Add(name, value, dbType, direction, size);
        }

        private static DbType ConvertGenericTypeToCustomType(DbGenericType type)
        {
            switch (type)
            {
                case DbGenericType.String: return DbType.String;
                case DbGenericType.Fraction: return DbType.Decimal;
                case DbGenericType.Number: return DbType.Int64;
                case DbGenericType.Bool: return DbType.Int16; //Silly, I know, but Oracle doesn't support Boolean types.
                case DbGenericType.DateTime: return DbType.DateTime;
                case DbGenericType.LargeObject: return DbType.Object;
                default: return DbType.String;
            }
        }

        public override ParameterInformation CustomizeParameterInformation(ParameterInformation p)
        {
            p.TargetDatabaseType = ConvertGenericTypeToCustomType(p.Type);
            return p;
        }
    }
}