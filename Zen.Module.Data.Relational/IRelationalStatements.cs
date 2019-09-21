using System.Collections.Generic;
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

        Dictionary<string, RelationalAdapter.MemberDescriptor> MemberDescriptors { get; }

        string KeyMember { get; set; }
        string KeyColumn { get; set; }

        Dictionary<string, KeyValuePair<string, string>> SchemaElements { get; set; }


        DbConnection GetConnection<T>() where T : Data<T>;
        void RenderSchemaEntityNames<T>() where T : Data<T>;
        void ValidateSchema<T>() where T : Data<T>;
    }
}