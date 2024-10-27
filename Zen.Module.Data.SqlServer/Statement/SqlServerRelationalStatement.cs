using Zen.Module.Data.Relational;

namespace Zen.Module.Data.SqlServer.Statement
{
    public class SqlServerRelationalStatements : RelationalStatements
    {
        public SqlServerRelationalStatements()
        {
            PaginationWrapper = "{OriginalQuery} OFFSET {StartPosition} ROWS FETCH NEXT {Size} ROWS ONLY";
        }
    }
}