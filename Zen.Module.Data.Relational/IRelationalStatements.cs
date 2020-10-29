using System.Collections.Generic;
using System.Data.Common;

namespace Zen.Module.Data.Relational
{
    public interface IRelationalStatements
    {
        bool UseIndependentStatementsForKeyExtraction { get; }
        bool UseNumericPrimaryKeyOnly { get; }
        bool UseOutputParameterForInsertedKeyExtraction { get; }
        RelationalStatements Statements { get; }
        string KeyMember { get; set; }
        string KeyColumn { get; set; }
        Dictionary<string, Dictionary<string, string>> SchemaElements { get; set; }

        DbConnection GetConnection();
        void RenderSchemaEntityNames();
        void ValidateSchema();
    }
}