using Zen.Base.Common;
using Zen.Base.Module.Data.Connection;

namespace Zen.Module.Data.MySql
{
    [Priority(Level = -2)]
    public class MySqlDefaultBundle : ConnectionBundlePrimitive
    {
        public MySqlDefaultBundle() => AdapterType = typeof(MySqlAdapter<>);
    }
}