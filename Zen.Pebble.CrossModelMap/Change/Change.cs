using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Zen.Base.Module;
using Zen.Base.Module.Data.Connection;

namespace Zen.Pebble.CrossModelMap.Change
{
    public class ChangeEntry<T> : Data<ChangeEntry<T>>, IStorageCollectionResolver
    {
        public enum EResult
        {
            Success,
            Fail
        }

        public enum EType
        {
            New,
            Update
        }

        private const string CollectionSuffix = "changeEntry";
        private static readonly Dictionary<Type, string> NameMap = new Dictionary<Type, string>();

        public ChangeEntry()
        {
            EvaluateConfiguration();
        }


        [Key] public string Id { get; set; } = Guid.NewGuid().ToString();
        public EType Type { get; set; }
        public string Checksum { get; set; }
        public DateTime Timestamp { get; private set; } = DateTime.Now;
        public T Model { get; set; }
        public EResult Result { get; set; }
        public string ResultMessage { get; set; }

        public string GetStorageCollectionName()
        {
            return $"{NameMap[typeof(T)]}#{CollectionSuffix}";
        }

        private void EvaluateConfiguration()
        {
            var invalidChars = "_+".ToCharArray();

            var sourceType = typeof(T);
            var referenceType = typeof(T);

            if (NameMap.ContainsKey(sourceType)) return;

            while (referenceType?.IsGenericType == true)
                referenceType = referenceType.GenericTypeArguments.FirstOrDefault();


            NameMap[sourceType] = invalidChars.Aggregate(referenceType?.Name,
                (current, invalidChar) => current.Replace(invalidChar, '.'));
        }

        public override void BeforeSave()
        {
            Timestamp = DateTime.Now;
        }

        public class ChangeEntryConfiguration
        {
            public string StorageName { get; set; }
        }
    }
}