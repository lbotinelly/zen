using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Zen.Base.Extension;
using Zen.Base.Module;
using Zen.Base.Module.Data;
using Zen.Base.Module.Data.Adapter;
using Zen.Base.Module.Data.Connection;
using Zen.Web.Data.Pipeline.Moderation.Shared;

namespace Zen.Web.App.Data.Pipeline.Moderation
{
    public class ModerationTask<T> : Data<ModerationTask<T>>, IStorageCollectionResolver where T : Data<T>
    {
        private const string CollectionSuffix = "modTask";

        public object Entry { get; set; }
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string SourceId { get; set; }
        public string AuthorLocator { get; set; } = Zen.App.Current.Orchestrator.Person?.Locator;
        public string ModeratorLocator { get; set; }
        public DateTime TimeStamp { get; set; } = DateTime.Now;
        public string Rationale { get; set; }
        public string Action { get; set; }
        public string GetStorageCollectionName() { return $"{Info<T>.Settings.StorageCollectionName}#{CollectionSuffix}"; }
        public new static DataAdapterPrimitive<T> GetDataAdapter() => Info<T>.Settings.Adapter;

        public List<ModerationHeader> GetModerationTasksById(string id)
        {
            var ret = Where(i => i.SourceId == id).ToJson().FromJson<List<ModerationHeader>>();
            return ret;
        }

        public static ModerationTask<T> Approve(string tid)
        {
            var hs = ModerationHelper.Setup<T>();
            var allowedActions = hs.ModerationActions;

            if (!allowedActions.Moderate) throw new AccessViolationException("Unauthorized: User isn't Moderator.");

            var preRet = Get(tid);

            var oProbe = (Data<T>)preRet.Entry;

            if (hs.Setup.StatusModelMember != null)
                if (!oProbe.SetMemberValue(hs.Setup.StatusModelMember, States.ResultLabel.Approved)) Base.Current.Log.Warn<T>($"Could NOT change moderation status field {hs.Setup.StatusModelMember} on [{preRet.SourceId}]");
                else if (hs.Setup.ActivityModelMember != null) oProbe.SetMemberValue(hs.Setup.ActivityModelMember, true);

            oProbe.Save();

            preRet.Remove();

            new ModerationLog<T>
            {
                SourceId = oProbe.GetDataKey(),
                Entry = oProbe,
                Action = preRet.Action,
                Result = States.EResult.Approved
            }.Save();
            Base.Current.Log.Info<T>($"MODERATION TASK ACCEPTED: {tid} ");

            return preRet;
        }

        public static ModerationTask<T> Reject(string tid)
        {
            var hs = ModerationHelper.Setup<T>();
            var allowedActions = hs.ModerationActions;

            if (!allowedActions.Moderate) throw new AccessViolationException("Unauthorized: User isn't Moderator.");

            var preRet = Get(tid);

            if (hs.Setup.StatusModelMember != null)
            {
                var oProbe = Data<T>.Get(preRet.SourceId);

                if (oProbe != null)
                {
                    if (!oProbe.SetMemberValue(hs.Setup.StatusModelMember, States.ResultLabel.Rejected)) Base.Current.Log.Warn<T>($"Could NOT change moderation status field {hs.Setup.StatusModelMember} on [{preRet.SourceId}]");
                    else oProbe.Save();
                }
            }

            preRet.Remove();

            new ModerationLog<T>
            {
                SourceId = ((Data<T>)preRet.Entry).GetDataKey(),
                Entry = preRet.Entry,
                Action = preRet.Action,
                AuthorLocator = preRet.AuthorLocator,
                ModeratorLocator = Zen.App.Current.Orchestrator.Person?.Locator,
                Result = States.EResult.Rejected
            }.Save();

            Base.Current.Log.Info<T>($"MODERATION TASK REJECTED: {tid}");

            return preRet;
        }

        public static ModerationTask<T> Withdraw(string tid)
        {
            var preRet = Get(tid);
            var hs = ModerationHelper.Setup<T>();

            var allowedActions = hs.ModerationActions;

            var can = allowedActions.Moderate || preRet.AuthorLocator == Zen.App.Current.Orchestrator.Person?.Locator;

            if (!can) throw new AccessViolationException("Unauthorized: User isn't Author or Moderator.");

            preRet.Remove();

            new ModerationLog<T>
            {
                SourceId = ((Data<T>)preRet.Entry).GetDataKey(),
                Entry = preRet.Entry,
                Action = preRet.Action,
                AuthorLocator = preRet.AuthorLocator,
                ModeratorLocator = Zen.App.Current.Orchestrator.Person?.Locator,
                Result = States.EResult.Withdrawn
            }.Save();

            Base.Current.Log.Info<T>($"MODERATION TASK WITHDRAWN: {tid}");

            return preRet;
        }
    }
}