using System.Collections.Generic;
using System.Linq;

namespace Zen.Base.Module.Log
{
    public static class Extensions
    {
        public static Clicker GetClicker<T>(this IEnumerable<T> source, string message, bool useClassName = true) => useClassName ? new Clicker($"[{typeof(T).Name}] " + message, source.Count()) : new Clicker(message, source.Count());
    }
}