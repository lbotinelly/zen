using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Zen.Base.Module.Data.LINQ
{
    public class DataContext<T> : IOrderedQueryable<T> where T : Data<T>
    {
        public DataContext()
        {
            //System.Console.WriteLine("FileSystemContext()");
            Provider = new DataProvider<T>();
            Expression = Expression.Constant(this);
        }

        internal DataContext(IQueryProvider provider, Expression expression)
        {
            Provider = provider;
            Expression = expression;
        }

        /// <summary>
        ///     Return a type-safe Enumerator.
        ///     <remarks>Unfortunately framework wants a non-generic Enumerator.</remarks>
        /// </summary>
        /// <returns>IEnumerator</returns>
        public IEnumerator<T> GetEnumerator()
        {
            //System.Console.WriteLine("GetEnumerator(1)");
            return Provider.Execute<IEnumerable<T>>(Expression).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            //System.Console.WriteLine("GetEnumerator(2)");
            // call the generic version of the method
            return GetEnumerator();
        }

        public Type ElementType => typeof(T);

        public Expression Expression { get; }
        public IQueryProvider Provider { get; }
    }
}