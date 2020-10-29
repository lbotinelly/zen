using System;
using System.Collections.Generic;

namespace Zen.Pebble.Database.Common
{
    public class ModelDescriptor
    {
        public CollectionDescriptor Collection = new CollectionDescriptor();

        public Dictionary<string, MemberDescriptor> Members = new Dictionary<string, MemberDescriptor>();

        public class MemberDescriptor
        {
            public int? Length;
            public string TargetName;
            public string SourceName;
            public Type ValueType;
            public EMemberSourceType MemberSourceType;

            public enum EMemberSourceType
            {
                Property,
                Field
            }
        }

        public class CollectionDescriptor
        {
            public string SourceName;
            public string TargetName;
        }
    }
}