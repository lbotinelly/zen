using Zen.Pebble.Database.Renders.Relational;

namespace Zen.Module.Data.Oracle.Statement
{
    public class OracleStatementFragments : RelationalStatementFragments
    {
        public override string ParametrizedValue => ":p{0}";
    }
}