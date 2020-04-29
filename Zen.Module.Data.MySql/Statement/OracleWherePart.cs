using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Zen.Pebble.Database.Common;
using Zen.Pebble.Database.Renders;

namespace Zen.Module.Data.MySql.Statement
{
    public class OracleWherePart : WherePartPrimitive<OracleWherePart>
    {
        public string Statement { get; set; }
        public Dictionary<string, object> Parameters { get; set; } = new Dictionary<string, object>();

        public string ParameterFormat { get; set; }

        public IWherePart IsSql(string sql, Dictionary<string, object> parameters = null)
        {
            return new OracleWherePart
            {
                Parameters = parameters ?? new Dictionary<string, object>(),
                Statement = sql
            };
        }

        public IWherePart IsParameter(int count, object value)
        {
            return new OracleWherePart
            {
                Parameters = {{count.ToString(), value}},
                Statement = $"@{count}"
            };
        }

        public IWherePart IsParameter(string field, object value)
        {
            return new OracleWherePart
            {
                Parameters = {{field, value}},
                Statement = field
            };
        }

        public IWherePart IsCollection(ref int countStart, IEnumerable values)
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
            sql[^1] = ')';
            return new OracleWherePart
            {
                Parameters = parameters,
                Statement = sql.ToString()
            };
        }

        public IWherePart Concat(string @operator, IWherePart operand)
        {
            return new OracleWherePart
            {
                Parameters = operand.Parameters,
                Statement = $"( {@operator.Format("", operand.Statement).Trim()} )"
            };
        }

        public IWherePart Concat(IWherePart left, string @operator, IWherePart right)
        {
            return new OracleWherePart
            {
                Parameters = left.Parameters.Union(right.Parameters).ToDictionary(kvp => kvp.Key, kvp => kvp.Value),
                Statement = $"( {@operator.Format(left.Statement, right.Statement).Trim()} )"
            };
        }
    }
}