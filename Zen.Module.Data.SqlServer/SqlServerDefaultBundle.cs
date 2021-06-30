using Zen.Base.Common;
using Zen.Base.Module.Data.Connection;

namespace Zen.Module.Data.SqlServer
{
    [Priority(Level = -2)]
    public class SqlServerDefaultBundle : ConnectionBundlePrimitive
    {
        public SqlServerDefaultBundle() => AdapterType = typeof(SqlServerAdapter<>);
    }
}
