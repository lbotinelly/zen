﻿using System.Collections.Generic;
using System.Linq;

namespace Zen.Pebble.CrossModelMap.Change
{
    public static class Extensions
    {
        public static void Save<T>(this Dictionary<string, ChangeEntry<T>> source)
        {
            var entries = source.Values.ToList();
            ChangeEntry<T>.Save(entries);
        }
    }
}