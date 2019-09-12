using System.Data.Common;
using Zen.Base.Module;

namespace Zen.Module.Data.Relational
{
    public interface IRelationalStatements
    {
        bool UseIndependentStatementsForKeyExtraction { get; }
        bool UseNumericPrimaryKeyOnly { get; }
        bool UseOutputParameterForInsertedKeyExtraction { get; }
        RelationalStatements Statements { get; }
        DbConnection GetConnection<T>() where T : Data<T>;
    }
}