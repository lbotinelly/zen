using System;
using System.Linq.Expressions;

namespace Zen.Base.Module.Data.LINQ
{
    public class DataQueryContext<T> where T : Data<T>
    {
        internal static object Execute(Expression expression, bool isEnumerable)
        {
            return Data<T>.Where(expression.GetLambda<T>());
        }
    }

    public static class Utils
    {
        public static Expression<Func<T, bool>> GetLambda<T>(this Expression expression)
        {
            switch (expression)
            {
                case MethodCallExpression callExpression:
                    return callExpression.GetLambda<T>();
                case UnaryExpression unaryExpression:
                    return unaryExpression.GetLambda<T>();
                default:
                    return null;
            }
        }

        public static Expression<Func<T, bool>> GetLambda<T>(this MethodCallExpression expression)
        {
            var unaryExpression = (UnaryExpression) expression.Arguments[1];
            return unaryExpression.GetLambda<T>();
        }

        public static Expression<Func<T, bool>> GetLambda<T>(this UnaryExpression expression)
        {
            return (Expression<Func<T, bool>>) expression.Operand;
        }
    }
}