using System;
using System.Linq.Expressions;

namespace Zen.Base.Module.Data.LINQ
{
    public static class Extensions
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