using System.Collections.Generic;
using System.Data.Common;
using Zen.Module.Data.Relational.Common;

namespace Zen.Module.Data.Relational
{
    public interface IRelationalStatements
    {
        bool UseIndependentStatementsForKeyExtraction { get; }
        bool UseNumericPrimaryKeyOnly { get; }
        bool UseOutputParameterForInsertedKeyExtraction { get; }
        RelationalStatements Statements { get; }

        Dictionary<string, MemberDescriptor> MemberDescriptors { get; }

        string KeyMember { get; set; }
        string KeyColumn { get; set; }

        Dictionary<string, KeyValuePair<string, string>> SchemaElements { get; set; }

        DbConnection GetConnection();
        void RenderSchemaEntityNames();
        void ValidateSchema();
    }
}