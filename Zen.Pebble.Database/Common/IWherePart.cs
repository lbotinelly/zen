using System.Collections;
using System.Collections.Generic;

namespace Zen.Pebble.Database.Common
{
    public interface IWherePart
    {
        string Statement { get; set; }
        Dictionary<string, object> Parameters { get; set; }
        string ParameterFormat { get; set; }
        IWherePart IsSql(string sql, Dictionary<string, object> parameters = null);
        IWherePart IsParameter(int count, object value);
        IWherePart IsParameter(string field, object value);
        IWherePart IsCollection(ref int countStart, IEnumerable values);
        IWherePart Concat(string @operator, IWherePart operand);
        IWherePart Concat(IWherePart left, string @operator, IWherePart right);
    }
}