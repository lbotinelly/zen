using System.Linq.Expressions;

namespace Zen.Base.Module.Data.LINQ
{
    public class DataQueryContext<T> where T : Data<T>
    {
        internal static object Execute(Expression expression, bool isEnumerable)
        {
            return Data<T>.Where(expression.AsLambda<T>());
        }
    }
}