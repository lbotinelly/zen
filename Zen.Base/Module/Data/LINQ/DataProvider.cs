using System.Linq;
using System.Linq.Expressions;

namespace Zen.Base.Module.Data.LINQ
{
    public class DataProvider<T> : IQueryProvider where T : Data<T>
    {
        public IQueryable CreateQuery(Expression expression)
        {
            return new DataContext<T>(this, expression);
        }

        public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
        {
            return (IQueryable<TElement>) new DataContext<T>(this, expression);
        }

        public object Execute(Expression expression)
        {
            return Execute<T>(expression);
        }

        public TResult Execute<TResult>(Expression expression)
        {
            var isEnumerable = typeof(TResult).Name == "IEnumerable`1";
            return (TResult) DataQueryContext<T>.Execute(expression, isEnumerable);
        }
    }
}