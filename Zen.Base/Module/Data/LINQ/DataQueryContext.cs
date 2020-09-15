using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;

namespace Zen.Base.Module.Data.LINQ
{
    public class DataQueryContext<T> where T:Data<T>
    {
        internal static object Execute(Expression expression, bool isEnumerable)
        {

            var p1 = Expression.Parameter(typeof(T));
            var expressionLambda = Expression.Lambda<Func<T, bool>>(expression, p1);

            return Data<T>.Where(expressionLambda);
        }
     }


}
