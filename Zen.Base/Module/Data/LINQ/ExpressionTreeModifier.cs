using System;
using System.Linq;
using System.Linq.Expressions;

namespace Zen.Base.Module.Data.LINQ
{
    internal class ExpressionTreeModifier<T> : ExpressionVisitor where T : Data<T>
    {
        private readonly IQueryable<T> _modelSet;

        internal ExpressionTreeModifier(IQueryable<T> elements)
        {
            //System.Console.WriteLine("ExpressionTreeModifier()");
            _modelSet = elements;
        }

        internal Expression CopyAndModify(Expression expression)
        {
            Console.WriteLine("CopyAndModify()");
            return Visit(expression);
        }

        protected override Expression VisitConstant(ConstantExpression c)
        {
            //StringBuilder s = new StringBuilder ();
            //s.AppendFormat("CopyAndModify.VisitConstant expression type {0}", c.Type.ToString());
            //System.Console.WriteLine(s.ToString ());
            // if (c.Type == typeof(IQueryable<LinqFileSystemProvider.FileSystemContext>))
            return c.Type == typeof(DataContext<T>) ? Expression.Constant(_modelSet) : c;
        }
    }
}