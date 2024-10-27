using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;

namespace Zen.Base.Module.Log
{
    public static class Extensions
    {
        public static Clicker GetClicker<T>(this IEnumerable<T> source, string message, bool useClassName = true)
        {
            return useClassName
                ? new Clicker($"[{typeof(T).Name}] " + message, source.Count())
                : new Clicker(message, source.Count());
        }


        public static void Click(this ConcurrentDictionary<string, long> source, string tag)
        {
            if (!source.ContainsKey(tag)) source.TryAdd(tag, 0);
            source[tag]++;
        }

        public static void Click<T>(this ConcurrentDictionary<string, long> source, string tag, IEnumerable<T> collection) { source.Click(tag, collection.Count()); }

        public static void Click(this ConcurrentDictionary<string, long> source, string tag, long count)
        {
            if (!source.ContainsKey(tag)) source.TryAdd(tag, 0);

            source[tag] += count;
            if (count > 1) Current.Log.KeyValuePair($"{tag}", count.ToString(), Message.EContentType.Info);
        }

        public static void ToLog(this ConcurrentDictionary<string, long> source, Message.EContentType type = Message.EContentType.MoreInfo)
        {
            if (source.Count <= 0) return;

            foreach (var key in source.Keys) Base.Log.KeyValuePair(key, source[key].ToString(), type);
        }

    }
}