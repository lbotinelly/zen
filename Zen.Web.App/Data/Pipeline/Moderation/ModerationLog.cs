using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Zen.App.Data.Log;
using Zen.Base.Extension;
using Zen.Base.Module;
using Zen.Base.Module.Data;
using Zen.Base.Module.Data.Adapter;
using Zen.Base.Module.Data.Connection;
using Zen.Web.Data.Pipeline.Moderation.Shared;

namespace Zen.Web.App.Data.Pipeline.Moderation
{
    public class ModerationLog<T> : Data<ModerationLog<T>>, IStorageCollectionResolver, IModerationHeader where T : Data<T>
    {
        private const string CollectionSuffix = "modLog";
        public States.EResult Result { get; set; }
        public object Entry { get; set; }
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string ModeratorLocator { get; set; }
        public string SourceId { get; set; }
        public string AuthorLocator { get; set; } = Zen.App.Current.Orchestrator.Person?.Locator;
        public DateTime TimeStamp { get; set; } = DateTime.Now;
        public string Rationale { get; set; }
        public string Action { get; set; }
        public string GetStorageCollectionName() { return $"{Info<T>.Settings.StorageCollectionName}#{CollectionSuffix}"; }

        public new static DataAdapterPrimitive<T> GetDataAdapter() { return Info<T>.Settings.Adapter; }

        public List<ModerationHeader> GetModerationLogsById(string id) { return Where(i => i.SourceId == id).ToJson().FromJson<List<ModerationHeader>>(); }

        #region Overrides of Data<ModerationLog<T>>

        public override void AfterSave(string newKey)
        {
            new Log<T>
            {
                ReferenceId = SourceId,
                AuthorLocator = AuthorLocator,
                Action = Action,
                Type = "MODERATION"
            }.Save();
        }

        #endregion
    }
}