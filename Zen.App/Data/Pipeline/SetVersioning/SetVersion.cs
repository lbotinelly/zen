using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Zen.App.Data.Log;
using Zen.Base.Module;
using Zen.Base.Module.Data;
using Zen.Base.Module.Data.Adapter;
using Zen.Base.Module.Data.Connection;

namespace Zen.App.Data.Pipeline.SetVersioning
{
    public class SetVersion<T> : Data<SetVersion<T>>, IStorageCollectionResolver where T : Data<T>
    {
        private const string CollectionSuffix = "set";

        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Code { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsPublic { get; set; }
        public bool IsLocked { get; set; }
        public bool IsCurrent { get; set; }
        public long ItemCount { get; set; }
        public DateTime TimeStamp { get; set; } = DateTime.Now;
        public string OperatorLocator { get; set; } = Current.Orchestrator?.Person?.Locator;

        public static SetVersioningPrimitiveAttribute Configuration { get; } = typeof(T).GetCustomAttributes(typeof(SetVersioningPrimitiveAttribute), true).FirstOrDefault() as SetVersioningPrimitiveAttribute;

        // public override void OnRemove() { LocalCache.Delete(GetItemCacheKey()); }
        public string SetTag => Id == Constants.CURRENT_LIVE_WORKSET_TAG ? null : $"{CollectionSuffix}:{Id}";

        public string GetStorageCollectionName() => $"{Info<T>.Settings.StorageCollectionName}#{CollectionSuffix}";

        public new static DataAdapterPrimitive<T> GetDataAdapter() => Info<T>.Settings.Adapter;

        public override void BeforeSave()
        {
            var isNew = IsNew();

            var action = isNew ? "Created" : "Updated";

            if (Current.Orchestrator?.Person!= null) action += " by " + Current.Orchestrator?.Person.Name + " (" + Current.Orchestrator?.Person.Locator + ")";

            var log = new Log<T>
            {
                ReferenceId = Id,
                AuthorLocator = Current.Orchestrator?.Person?.Locator,
                Action = isNew ? "CREATE" : "UPDATE",
                Type = Log.Constants.Type.VERSIONING,
                Message = $"Version [{Code}] ({Name}) {action}"
            };

            log.Save();
        }

        public override void BeforeRemove()
        {
            Info<T>.Settings.Adapter.DropSet(Id);

            var action = "Dropped";

            var person = Current.Orchestrator?.Person;

            if (person!= null) action += " by " + person.Name + " (" + person.Locator + ")";

            var log = new Log<T>
            {
                ReferenceId = Id,
                AuthorLocator = person?.Locator,
                Action = "DROP",
                Type = Log.Constants.Type.VERSIONING,
                Message = $"Version [{Code}] ({Name}) {action}"
            };

            log.Save();
        }

        private static string CollectionTag(string id) { return $"{CollectionSuffix}:{id}"; }

        private IEnumerable<T> VersionGetAll()
        {
            var set = Code == Constants.CURRENT_LIVE_WORKSET_TAG ? "" : CollectionTag(Id);

            Base.Log.Add<T>($"VersionGetAll {set ?? "(none)"}");
            var mutator = new Mutator {SetCode = set};

            return Data<T>.Query(mutator);
        }

        public static SetVersion<T> PushFromWorkset(string id)
        {
            var isNew = id == "new" || id == Constants.CURRENT_LIVE_WORKSET_TAG;

            SetVersion<T> probe;

            if (isNew)
            {
                var code = DateTime.Now.ToString("yyyyMMdd-HHmmss");
                probe = new SetVersion<T> {Code = "BKP" + code, Name = "Backup:" + code};
            }
            else
            {
                probe = Get(id);
                if (probe == null) throw new Exception($"Version {id} not found.");
            }

            try
            {
                probe.ItemCount = Data<T>.Count();
                probe.Save();

                Info<T>.Settings.Adapter.CopySet(Constants.CURRENT_LIVE_WORKSET_TAG, CollectionTag(probe.Id), true);

                var log = new Log<T>
                {
                    ReferenceId = probe.Id,
                    AuthorLocator = Current.Orchestrator?.Person?.Locator,
                    Action = "PUSH",
                    Type = Log.Constants.Type.VERSIONING,
                    Message = $"Workset pushed to Version [{probe.Code}] ({probe.Name}): {probe.ItemCount} items copied"
                };
                log.Save();

                return probe;
            } catch (Exception e) { throw e; }
        }

        public Payload GetPackage()
        {
            var ret = new Payload
            {
                Content = Info<T>.Configuration.Description ?? typeof(T).FullName,
                Descriptor = this,
                Items = VersionGetAll()
            };

            return ret;
        }

        public static Payload GetPackage(string code)
        {
            var package = GetByCode(code).GetPackage();
            return package;
        }

        public void PullToWorkset()
        {
            Info<T>.Settings.Adapter.CopySet(CollectionTag(Id), Constants.CURRENT_LIVE_WORKSET_TAG, true);

            new Log<T>
            {
                ReferenceId = Id,
                AuthorLocator = Current.Orchestrator?.Person?.Locator,
                Action = "PULL",
                Type = Log.Constants.Type.VERSIONING,
                Message = $"Version [{Code}] ({Name}) pulled to Workset"
            }.Save();
        }

        public static SetVersion<T> GetByCode(string code)
        {
            return string.IsNullOrEmpty(code) || code == Constants.CURRENT_LIVE_WORKSET_TAG ? new SetVersion<T> {Id = Constants.CURRENT_LIVE_WORKSET_TAG, Code = Constants.CURRENT_LIVE_WORKSET_TAG} : Where(i => i.Code == code).FirstOrDefault();
        }

        public static bool CanModify() { return Configuration.CanModify(); }
        public static bool CanBrowse() { return Configuration.CanBrowse(); }

        public class Payload
        {
            public string Content { get; set; }
            public SetVersion<T> Descriptor { get; set; }
            public IEnumerable<object> Items { get; set; }
        }
    }
}