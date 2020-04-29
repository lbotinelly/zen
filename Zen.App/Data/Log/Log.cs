using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Zen.Base.Extension;
using Zen.Base.Module;
using Zen.Base.Module.Data;
using Zen.Base.Module.Data.Adapter;
using Zen.Base.Module.Data.Connection;

namespace Zen.App.Data.Log
{
    [DataConfig(UseCaching = false)]
    public class Log<T> : Data<Log<T>>, IStorageCollectionResolver where T : Data<T>
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Action { get; set; } = Constants.Type.UNDEFINED;
        public string Type { get; set; } = Constants.Type.UNDEFINED;
        public string AuthorLocator { get; set; } = Current.Orchestrator.Person?.Locator;
        public string ReferenceId { get; set; }
        public string Message { get; set; }
        public DateTime TimeStamp { get; set; } = DateTime.Now;

        public List<Header> GetLogsById(string id)
        {
            var ret = Where(i => i.ReferenceId == id)
                .ToJson()
                .FromJson<List<Header>>();
            return ret;
        }

        #region Implementation of IStorageCollectionResolver

        public string GetStorageCollectionName() => $"{Info<T>.Settings.StorageCollectionName}#log";
        public new static DataAdapterPrimitive<T> GetDataAdapter() { return Info<T>.Settings.Adapter; }

        #endregion
    }
}