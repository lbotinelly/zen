using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Zen.Module.Data.Relational.Builder
{
    // http://ryanohs.com/2016/04/generating-sql-from-expression-trees-part-2/

    public class WherePart
    {
        public string Sql { get; set; }
        public Dictionary<string, object> Parameters { get; set; } = new Dictionary<string, object>();

        public string ParameterFormat { get; set; }

        public static WherePart IsSql(string sql, Dictionary<string, object> parameters = null)
        {
            return new WherePart
            {
                Parameters = parameters ?? new Dictionary<string, object>(),
                Sql = sql
            };
        }

        public static WherePart IsParameter(int count, object value)
        {
            return new WherePart
            {
                Parameters = {{count.ToString(), value}},
                Sql = $"@{count}"
            };
        }

        public static WherePart IsParameter(string field, object value)
        {
            return new WherePart
            {
                Parameters = {{field, value}},
                Sql = field
            };
        }

        public static WherePart IsCollection(ref int countStart, IEnumerable values)
        {
            var parameters = new Dictionary<string, object>();
            var sql = new StringBuilder("(");
            foreach (var value in values)
            {
                parameters.Add(countStart.ToString(), value);
                sql.Append($"@{countStart},");
                countStart++;
            }

            if (sql.Length == 1) sql.Append("null,");
            sql[sql.Length - 1] = ')';
            return new WherePart
            {
                Parameters = parameters,
                Sql = sql.ToString()
            };
        }

        public static WherePart Concat(string @operator, WherePart operand)
        {
            return new WherePart
            {
                Parameters = operand.Parameters,
                Sql = $"({@operator} {operand.Sql})"
            };
        }

        public static WherePart Concat(WherePart left, string @operator, WherePart right)
        {
            return new WherePart
            {
                Parameters = left.Parameters.Union(right.Parameters).ToDictionary(kvp => kvp.Key, kvp => kvp.Value),
                Sql = $"({left.Sql} {@operator} {right.Sql})"
            };
        }
    }
}