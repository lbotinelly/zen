using System;
using System.Linq.Expressions;
using Zen.Pebble.Database.Common;

namespace Zen.Pebble.Database {
    public interface IModelRender<T, TFragments, TWherePart> where TFragments : IStatementFragments where TWherePart : IWherePart
    {
        StatementMasks Masks { get; set; }
        IWherePart Render(Expression<Func<T, bool>> expression);
    }
}