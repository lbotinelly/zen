using System;
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

    public static class Utils
    {
        public static Expression<Func<T, bool>> AsLambda<T>(this Expression expression)
        {
            // Strict call on Typecast
            switch (expression)
            {
                case MethodCallExpression methodCallExpression:
                    return methodCallExpression.AsLambda<T>();
                case UnaryExpression unaryExpression:
                    return unaryExpression.AsLambda<T>();
                default:
                    return null;
            }
        }

        public static Expression<Func<T, bool>> AsLambda<T>(this MethodCallExpression expression)
        {
            // Extract the unary expression from the arguments
            var unaryExpression = (UnaryExpression) expression.Arguments[1];
            return unaryExpression.AsLambda<T>();
        }

        public static Expression<Func<T, bool>> AsLambda<T>(this UnaryExpression expression)
        {
            return (Expression<Func<T, bool>>) expression.Operand;
        }
    }
}