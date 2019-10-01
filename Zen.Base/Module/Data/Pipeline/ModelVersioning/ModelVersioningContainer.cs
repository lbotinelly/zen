using System;
using System.ComponentModel.DataAnnotations;
using Zen.Base.Module.Data.Adapter;
using Zen.Base.Module.Data.Connection;

namespace Zen.Base.Module.Data.Pipeline.ModelVersioning
{
    public class ModelVersioningContainer<T> : Data<ModelVersioningContainer<T>>, IStorageCollectionResolver where T : Data<T>
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public Data<T> Entry { get; set; }
        public DateTime TimeStamp { get; set; } = DateTime.Now;
        public EActionType Action { get; set; }
        public string Summary { get; set; }
        public string SourceId { get; set; }
        public string GetStorageCollectionName() { return $"{Info<T>.Settings.StorageName}#log"; }

        public new static DataAdapterPrimitive GetDataAdapter() { return Info<T>.Settings.Adapter; }
    }
}