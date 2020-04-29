using Zen.Pebble.Database;

namespace Zen.Module.Data.MySql.Statement
{
    public class OracleStatement<T> : StatementRender<OracleStatementFragments, OracleWherePart, T> { }
}