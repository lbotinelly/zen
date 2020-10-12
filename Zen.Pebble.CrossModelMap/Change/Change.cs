using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Zen.Base.Extension;
using Zen.Base.Module;
using Zen.Base.Module.Data.Connection;

namespace Zen.Pebble.CrossModelMap.Change
{
    public class ChangeEntry<T> : Data<ChangeEntry<T>>, IStorageCollectionResolver
    {
        public enum EType
        {
            New,
            Update
        }

        private const string CollectionSuffix = "changeEntry";
        private static Dictionary<Type, string> _nameMap = new Dictionary<Type, string>();

        public ChangeEntry()
        {

            var sourceType = typeof(T);
            var referenceType = typeof(T);

            if (_nameMap.ContainsKey(sourceType)) return;

            var invalidChars = "_+".ToCharArray();

            while (referenceType?.IsGenericType == true)
                referenceType = referenceType.GenericTypeArguments.FirstOrDefault();

            var name = invalidChars.Aggregate(referenceType?.FullName, (current, invalidChar) => current.Replace(invalidChar, '.'));

            _nameMap[sourceType] = name;

        }


        [Key] public string Id { get; set; } = Guid.NewGuid().ToString();
        public EType Type { get; set; }
        public string Checksum { get; set; }
        public DateTime Timestamp { get; private set; } = DateTime.Now;
        public T Model { get; set; }

        public string GetStorageCollectionName()
        {
            return $"{_nameMap[typeof(T)]}#{CollectionSuffix}";
        }

        public override void BeforeSave()
        {
            Timestamp = DateTime.Now;
        }
    }
}