using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Zen.Base.Module.Log
{



    public class TagClicker : ConcurrentDictionary<string, long>
    {


        private readonly string _suffix;
        private int _maxLength;

        public TagClicker() { }

        public TagClicker(string suffix) { _suffix = suffix; }

        public new long this[string tag]
        {
            get
            {
                if (ContainsKey(tag)) return base[tag];

                TryAdd(tag, 0);
                if (tag.Length > _maxLength) _maxLength = tag.Length;
                return base[tag];
            }
            set
            {
                if (!ContainsKey(tag))
                {
                    TryAdd(tag, 0);
                    if (tag.Length > _maxLength) _maxLength = tag.Length;
                }

                base[tag] = value;
            }
        }
        public void ToLog(Message.EContentType type = Message.EContentType.MoreInfo)
        {
            if (Keys.Count <= 0) return;

            foreach (var key in Keys) Base.Log.KeyValuePair((_suffix!= null ? " " + _suffix : "") + key, base[key].ToString(), type);
        }
    }
}