using System;
using System.ComponentModel.DataAnnotations;
using Zen.Base.Module;
using Zen.Base.Module.Data;
using Zen.Base.Module.Data.Adapter;
using Zen.Base.Module.Data.Connection;

namespace Zen.App.Data.Pipeline.ModelVersioning
{
    public class ModelVersioning<T> : Data<ModelVersioning<T>>, IStorageCollectionResolver where T : Data<T>
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public DateTime TimeStamp { get; set; } = DateTime.Now;
        public Data<T> Entry { get; set; }
        public EActionType Action { get; set; }
        public string Summary { get; set; }
        public string SourceId { get; set; }
        public string PersonLocator { get; set; } = Current.Orchestrator?.Person?.Locator;

        public string GetStorageCollectionName() => $"{Info<T>.Settings.StorageCollectionName}#ver";
        public new static DataAdapterPrimitive<T> GetDataAdapter() => Info<T>.Settings.Adapter;
    }
}