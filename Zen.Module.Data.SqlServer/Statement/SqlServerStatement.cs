using Zen.Pebble.Database;

namespace Zen.Module.Data.SqlServer.Statement
{
    public class SqlServerStatement<T> : ModelRender<T, SqlServerStatementFragments, SqlServerWherePart> { }
}