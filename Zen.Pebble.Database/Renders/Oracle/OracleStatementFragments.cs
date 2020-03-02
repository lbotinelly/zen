using Zen.Pebble.Database.Renders.Relational;

namespace Zen.Pebble.Database.Renders.Oracle
{
    public class OracleStatementFragments : RelationalStatementFragments
    {
        public override string ParametrizedValue => ":p{0}";
    }
}