using Zen.Pebble.Database.Renders.Relational;

namespace Zen.Module.Data.SqlServer.Statement
{
    public class SqlServerStatementFragments : RelationalStatementFragments
    {
        public override string ParametrizedValue => "@{0}";
    }
}