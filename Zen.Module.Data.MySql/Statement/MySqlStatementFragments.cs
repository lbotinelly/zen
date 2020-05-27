using Zen.Pebble.Database.Renders.Relational;

namespace Zen.Module.Data.MySql.Statement
{
    public class MySqlStatementFragments : RelationalStatementFragments
    {
        public override string ParametrizedValue => ":p{0}";
    }
}