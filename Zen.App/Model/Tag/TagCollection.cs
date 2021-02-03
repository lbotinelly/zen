using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Zen.App.Model.Tag
{
    public class TagCollection : List<string>
    {
        public void Ensure(string item)
        {
            if (item.IndexOf(":", StringComparison.Ordinal) != -1)
            {
                var parts = item.Split(':');
                var keyword = parts[0];

                var probe = Find(a => a.StartsWith(keyword));

                if (probe!= null) base.Remove(probe);

                base.Add(item);
            }
            else
            {
                if (Find(a => a.Equals(item)) == null) base.Add(item);
            }
        }

        public string GetValue(string tag)
        {
            var probe = Find(a => a.StartsWith(tag));
            return probe?.Split(':')[1];
        }

        public new void Remove(string item)
        {
            if (item.IndexOf(":", StringComparison.Ordinal) != -1)
            {
                var parts = item.Split(':');
                var keyword = parts[0];

                var probe = Find(a => a.StartsWith(keyword));

                if (probe!= null) base.Remove(probe);
            }
            else { base.Add(item); }
        }
    }
}