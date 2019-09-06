using System;

namespace Zen.Storage.Model {
    public class ZenFileDescriptorAttribute : Attribute
    {
        public Type StorageType { get; set; }
    }
}