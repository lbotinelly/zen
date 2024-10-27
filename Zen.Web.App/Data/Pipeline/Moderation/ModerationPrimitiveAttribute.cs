using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Primitives;
using Zen.App.Communication;
using Zen.Base;
using Zen.Base.Extension;
using Zen.Base.Module;
using Zen.Base.Module.Data;
using Zen.Base.Module.Data.Pipeline;
using Zen.Web.Data.Pipeline.Moderation.Shared;

namespace Zen.Web.App.Data.Pipeline.Moderation
{
    [AttributeUsage(AttributeTargets.Class)]
    public abstract class ModerationPrimitiveAttribute : Attribute, IBeforeActionPipeline
    {
        public bool NotifyChanges { get; set; }
        public bool CreatorCanWithdraw { get; set; }

        public string ModeratorPermission { get; set; }
        public string WhitelistPermission { get; set; }
        public string NotifyPermission { get; set; }
        public string CreatorPermission { get; set; }

        public string CreatorLocatorModelMember { get; set; } = null;
        public string ImportModelMember { get; set; } = null;
        public string ActivityModelMember { get; set; } = null;
        public string StatusModelMember { get; set; } = null;

        public string PipelineName => "Moderation";

        #region Implementation of IPipelinePrimitive

        public virtual Dictionary<string, object> Headers<T>(ref DataAccessControl accessControl, Dictionary<string, StringValues> requestHeaders, EActionScope scope, T model) where T : Data<T>
        {
            var moderationSetup = ModerationHelper.Setup<T>();

            var ctx = new Dictionary<string, object> { { "moderated", true } };

            var customModerationPipeline = moderationSetup.CustomModerationPipeline;

            var allowedActions = moderationSetup.ModerationActions;

            if (customModerationPipeline != null && scope == EActionScope.Model) allowedActions = customModerationPipeline.GetModerationActions(EActionType.Read, scope, null, model, null) ?? allowedActions;

            if (allowedActions.Moderate) ctx.Add("moderator", true);
            if (allowedActions.Whitelisted) ctx.Add("whiteListed", true);
            if (allowedActions.Author) ctx.Add("canPost", true);

            if (customModerationPipeline != null)
            {
                var headerPayloadDictionary = new Dictionary<string, List<string>>();

                var onInsert = customModerationPipeline.OnInsertAbstracts();
                if (onInsert?.Any() == true) headerPayloadDictionary.Add("add", onInsert);

                var onUpdate = customModerationPipeline.OnUpdateAbstracts();
                if (onUpdate?.Any() == true) headerPayloadDictionary.Add("edit", onUpdate);

                var onRemove = customModerationPipeline.OnRemoveAbstracts();
                if (onRemove?.Any() == true) headerPayloadDictionary.Add("del", onRemove);

                if (headerPayloadDictionary.Any()) ctx.Add("abstract", headerPayloadDictionary);
            }

            var canShow = allowedActions.Read;
            ctx.Add("canShow", canShow);

            if (canShow)
            {
                var path = Web.Current.Context.Request.Path.ToUriComponent();
                path = path.Remove(path.Length - 1);

                if (path.Contains("moderation/task")) path = path.Replace("moderation/task", "");

                ctx.Add("baseUrl", path);
            }

            var ret = new Dictionary<string, object> { { "x-moderation", ctx } };

            return ret;
        }

        #endregion

        #region Implementation of IBeforeActionPipeline

        public T Process<T>(EActionType type, EActionScope scope, Mutator mutator, T current, T source) where T : Data<T>
        {
            // This is the main Moderation control flow. Let's start by setting up some helper classes:

            var moderationSetup = ModerationHelper.Setup<T>();
            var customModerationPipeline = moderationSetup.CustomModerationPipeline;
            var allowedActions = customModerationPipeline?.GetModerationActions(type, scope, mutator, current, source) ?? moderationSetup.ModerationActions;
            var currentPerson = Zen.App.Current.Orchestrator.Person?.Locator;

            //Log.Info<T>($"MOD {type} | {currentPerson}");
            //Log.Info<T>($"MOD {type} | {allowedActions.ToJson()}");
            //Log.Info<T>($"MOD {type} | {current.ToReference()}");

            var currentKey = current.GetDataKey();

            // What can this user do?

            // Before anything, check if the target entry is marked as Imported. In that case the whole Moderation pipeline is sidestepped.

            var importModelMember = moderationSetup.Setup.ImportModelMember;
            if (importModelMember != null)
            {
                if (!current.HasMember(importModelMember))
                {
                    var msg = $"Moderation Pipeline: Invalid configuration - missing [{importModelMember}] ImportModelMember";
                    Base.Current.Log.Warn<T>(msg);
                    throw new InvalidOperationException(msg);
                }

                switch (type)
                {
                    case EActionType.Insert:
                        var status = current.GetMemberValue<bool>(importModelMember);
                        if (status) return current; // Imported record, nothing to be done here.
                        break;
                    case EActionType.Update:
                        var newStatus = current.GetMemberValue<bool>(importModelMember);
                        var oldStatus = source.GetMemberValue<bool>(importModelMember);

                        if (source != null)
                            if (newStatus != oldStatus)
                                throw new ArgumentException($"Moderation Pipeline: Import flag [{importModelMember}] cannot be changed.");

                        if (newStatus) return current;
                        break;
                }
            }

            // Let's determine the Creator of this record.

            string creatorLocator = null;

            var creatorLocatorModelMember = moderationSetup.Setup.CreatorLocatorModelMember;
            if (creatorLocatorModelMember != null)
            {
                if (!current.HasMember(creatorLocatorModelMember))
                {
                    var msg = $"Moderation Pipeline: Invalid configuration - missing [{creatorLocatorModelMember}] CreatorLocatorModelMember";
                    Base.Current.Log.Warn<T>(msg);
                    throw new InvalidOperationException(msg);
                }

                creatorLocator = current.GetMemberValue<string>(creatorLocatorModelMember);
            }

            var isCreator = creatorLocator == currentPerson;
            string oRationale = null;

            // There are two kinds of Moderation action that this pipeline can handle. One is a Moderation Task....

            var controlPayload = ModerationHelper.GetModerationControlPayload();
            if (controlPayload != null)
            {
                // This is a Moderation Task operation - say, like an User trying to delete their own entry,
                // or a Mod changing things. We can use some extra data to help with the process.

                var oTaskId = controlPayload?.___ModerationTaskId;
                oRationale = controlPayload?.___ModerationRationale;

                if (oTaskId != null)
                {
                    var oTask = ModerationTask<T>.Get(oTaskId);

                    switch (type)
                    {
                        case EActionType.Insert:
                        case EActionType.Update:
                            // Let's change the payload to the new content.

                            var _os = oTask.Entry.ToJson();
                            var _ns = current.ToJson();

                            if (_ns != _os)
                            {
                                oTask.Entry = current;
                                oTask.Save();

                                new ModerationLog<T>
                                {
                                    SourceId = currentKey,
                                    Entry = current,
                                    Action = type.ToString()
                                }.Save();

                                Base.Current.Log.Info<T>($"Moderation Pipeline: PATCH {oTaskId}");
                            }

                            // Moderator/Whitelisted? Accept it.

                            if (allowedActions.Moderate || allowedActions.Whitelisted || moderationSetup.Setup.CreatorCanWithdraw && isCreator) ModerationTask<T>.Approve(oTaskId);

                            break;
                        case EActionType.Remove:
                            if (allowedActions.Moderate || allowedActions.Whitelisted) ModerationTask<T>.Withdraw(oTaskId);
                            else ModerationTask<T>.Reject(oTaskId);
                            break;
                    }
                }
            }

            // The second kind is regular models posted by users. Let's determine their fate:

            var mustCreateTask = false;
            var clearModel = false;

            if (!(allowedActions.Author || allowedActions.Whitelisted)) throw new InvalidOperationException("Moderation Pipeline: Not authorized.");

            switch (type)
            {
                case EActionType.Insert:
                case EActionType.Update:

                    if (!(allowedActions.Moderate || allowedActions.Whitelisted)) mustCreateTask = true;

                    if (allowedActions.Whitelisted) // User is Whitelisted
                        if (type == EActionType.Update) // If it's an update, 
                            if (!isCreator) // Whitelisted user isn't the creator
                                mustCreateTask = true;

                    break;
                case EActionType.Remove:
                    if (allowedActions.Moderate) break;
                    if (allowedActions.Whitelisted && isCreator) break;
                    if (moderationSetup.Setup.CreatorCanWithdraw && isCreator) break;

                    mustCreateTask = true;

                    break;
                default: throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }

            if (mustCreateTask) // 
            {
                var statusModelMember = moderationSetup.Setup.StatusModelMember;
                if (statusModelMember != null)
                    if (current.HasMember(statusModelMember))
                        if (!current.SetMemberValue(statusModelMember, null)) // Mark as 'Waiting Moderation'.
                            Base.Current.Log.Warn<T>($"Could NOT change moderation status on [{statusModelMember}] | {type}, entry {{{currentKey}}}");

                var moderationTask = new ModerationTask<T>
                {
                    SourceId = currentKey,
                    Entry = current,
                    Action = type.ToString(),
                    AuthorLocator = currentPerson,
                    Rationale = oRationale
                };

                moderationTask.Save();

                clearModel = true;
            }

            new ModerationLog<T>
            {
                SourceId = currentKey,
                Entry = current,
                Action = type.ToString(),
                AuthorLocator = currentPerson,
                Result = mustCreateTask ? States.EResult.TaskCreated : States.EResult.Approved
            }.Save();

            if (moderationSetup.Setup.NotifyChanges)
            {
                var ePer = new Email();

                var emailContent =
                    "A new entry has been submitted.<br/><br/>" +
                    "Entry: " + current.ToReference() + "<br/>" +
                    "Creator: " + Zen.App.Current.Orchestrator.Person + "<br/><br/>";

                ePer.AddTo(Zen.App.Current.Orchestrator.Application.GetGroup("CUR"));
                ePer.AddCc(Zen.App.Current.Orchestrator.Application.GetGroup("MOD"));

                ePer.Title = $"{currentPerson}: {type} {Info<T>.Settings.FriendlyName}";

                emailContent += !mustCreateTask
                    ? "User was whitelisted: Action was automatically approved."
                    : "Action was submitted for moderation.";

                ePer.Content = emailContent;
                ePer.Send();
            }

            if (clearModel) current = null;
            return current;
        }

        public KeyValuePair<string, string>? ParseRequest<T>(Dictionary<string, List<string>> requestData) where T : Data<T>
        {
            // Moderation involves no key manipulation.
            return null;
        }

        #endregion
    }
}