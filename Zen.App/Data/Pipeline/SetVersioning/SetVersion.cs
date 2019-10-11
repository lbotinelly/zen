using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Zen.App.Data.Log;
using Zen.Base.Extension;
using Zen.Base.Module;
using Zen.Base.Module.Data;
using Zen.Base.Module.Data.Adapter;

namespace Zen.App.Data.Pipeline.SetVersioning
{
    public class SetVersion<T> : Data<SetVersion<T>> where T : Data<T>
    {
        private const string _SETPREFIX = "set";

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

        public string GetStorageCollectionName() { return $"{Info<T>.Settings.StorageName}#ver"; }

        public new static DataAdapterPrimitive GetDataAdapter() { return Info<T>.Settings.Adapter; }

        // public override void OnRemove() { LocalCache.Delete(GetItemCacheKey()); }

        public string GetItemCacheKey() => (Info<T>.Settings.TypeQualifiedName + ":" + Id).Sha512Hash();

        public override void BeforeSave()
        {
            var isNew = IsNew();

            var verb = isNew ? "created" : "updated";

            if (Current.Orchestrator?.Person != null) verb += " by " + Current.Orchestrator?.Person.Name + " (" + Current.Orchestrator?.Person.Locator + ")";

            var log = new Log<T>
            {
                ReferenceId = Id,
                AuthorLocator = Current.Orchestrator?.Person?.Locator,
                Action = isNew ? "CREATE" : "UPDATE",
                Type = Log.Constants.Type.VERSIONING,
                Message = $"Version [{Code}] ({Name}) {verb}"
            };

            log.Save();
        }

        public override void BeforeRemove()
        {
            Info<T>.Settings.Adapter.DropSet<T>($"{Id}");

            var verb = "dropped";

            var person = Current.Orchestrator?.Person;

            if (person != null) verb += " by " + person.Name + " (" + person.Locator + ")";

            var log = new Log<T>
            {
                ReferenceId = Id,
                AuthorLocator = person?.Locator,
                Action = "DROP",
                Type = Log.Constants.Type.VERSIONING,
                Message = $"Version [{Code}] ({Name}) {verb}"
            };

            log.Save();
        }

        public IEnumerable<T> VersionGetAll()
        {
            var set = Code == Constants.CURRENT_LIVE_WORKSET_TAG ? "" : $"#{_SETPREFIX}:{Id}";

            Base.Log.Add($"VersionGetAll: {set}");
            var mutator = new Mutator {SetCode = set};

            return Data<T>.Query(mutator);
        }

        public T VersionGetItem(string locator)
        {
            var set = Code == Constants.CURRENT_LIVE_WORKSET_TAG ? "" : $"#{_SETPREFIX}:{Id}";

            Base.Log.Add($"VersionGetAll: {set}");
            var mutator = new Mutator {SetCode = set};

            return Data<T>.Get(Id, mutator);
        }

        public static SetVersion<T> PushFromWorkspace(string id)
        {
            var isNew = id == "new";

            SetVersion<T> probe;

            if (isNew)
            {
                var code = DateTime.Now.ToString("yyyyMMddHHmmss");
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

                Info<T>.Settings.Adapter.CopySet<T>("", $"#{_SETPREFIX}:{probe.Id}", true);

                var log = new Log<T>
                {
                    ReferenceId = probe.Id,
                    AuthorLocator = Current.Orchestrator?.Person?.Locator,
                    Action = "PUSH",
                    Type = Log.Constants.Type.VERSIONING,
                    Message = $"Workspace pushed to Version [{probe.Code}] ({probe.Name}): {probe.ItemCount} items copied"
                };
                log.Save();

                return probe;
            } catch (Exception e) { throw e; }
        }

        public Payload GetPackage()
        {
            var ret = new Payload
            {
                Content = GetType().FullName,
                Descriptor = this,
                Items = VersionGetAll()
            };

            return ret;
        }

        public static Payload GetPackage(string id)
        {
            var package = Get(id).GetPackage();
            return package;
        }

        public void PullToWorkspace()
        {
            Info<T>.Settings.Adapter.CopySet<T>($"#{_SETPREFIX}:{Id}", "", true);

            new Log<T>
            {
                ReferenceId = Id,
                AuthorLocator = Current.Orchestrator?.Person?.Locator,
                Action = "PULL",
                Type = Log.Constants.Type.VERSIONING,
                Message = $"Version [{Code}] ({Name}) pulled to Workspace"
            }.Save();
        }

        public static SetVersion<T> GetByCode(string code) { return code == Constants.CURRENT_LIVE_WORKSET_TAG ? new SetVersion<T> {Id = Constants.CURRENT_LIVE_WORKSET_TAG, Code = Constants.CURRENT_LIVE_WORKSET_TAG} : Where(i => i.Code == code).FirstOrDefault(); }

        public class Payload
        {
            public string Content { get; set; }
            public SetVersion<T> Descriptor { get; set; }
            public IEnumerable<object> Items { get; set; }
        }
    }
}