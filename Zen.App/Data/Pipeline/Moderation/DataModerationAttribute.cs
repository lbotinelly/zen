using System;
using System.Collections.Generic;
using System.Linq;
using Zen.Base.Module;
using Zen.Base.Module.Data;
using Zen.Base.Module.Data.Pipeline;

namespace Zen.App.Data.Pipeline.Moderation
{
    [AttributeUsage(AttributeTargets.Class)]
    public class DataModerationAttribute : Attribute, IBeforeActionPipeline
    {
        public bool NotifyChanges { get; set; }
        public bool AuthorCanWithdraw { get; set; }
        public string ModeratorPermission { get; set; }
        public string WhitelistPermission { get; set; }
        public string NotifyPermission { get; set; }
        public string AuthorPermission { get; set; }
        public string PipelineName => "Moderation";

        #region Implementation of IPipelinePrimitive

        public Dictionary<string, object> Headers<T>() where T : Data<T>
        {
            var ctx = new Dictionary<string, object> {{"moderated", true}};

            if (CanModerate()) ctx.Add("moderator", true);
            if (IsWhitelisted()) ctx.Add("whiteListed", true);
            if (CanAuthor()) ctx.Add("canPost", true);

            if (typeof(IModerationAbstractProvider).IsAssignableFrom(typeof(T)))
            {
                var probe = (IModerationAbstractProvider) Info<T>.Instance;
                var def = new Dictionary<string, List<string>>();

                var onInsert = probe.OnInsertAbstracts();if (onInsert.Any()) def.Add("add", onInsert);
                var onUpdate = probe.OnUpdateAbstracts();if (onUpdate.Any()) def.Add("edit", onUpdate);
                var onRemove = probe.OnRemoveAbstracts();if (onRemove.Any()) def.Add("del", onRemove);

                ctx.Add("abstract", def);
            }

            var canShow = CanModerate() || IsWhitelisted() || CanAuthor();
            ctx.Add("canShow", canShow);

            var ret = new Dictionary<string, object> {{"x-baf-moderation", ctx}};

            return ret;
        }

        #endregion

        #region helpers

        public bool CanModerate()
        {
            var res = Current.Orchestrator.Person?.HasAnyPermissions(ModeratorPermission);
            return res.HasValue && res.Value;
        }

        public bool IsWhitelisted()
        {
            var res = Current.Orchestrator.Person?.HasAnyPermissions(WhitelistPermission);
            return res.HasValue && res.Value;
        }

        public bool CanAuthor()
        {
            var res = Current.Orchestrator.Person?.HasAnyPermissions(AuthorPermission) == true || IsWhitelisted();
            return res;
        }

        #endregion

        #region Implementation of IBeforeActionPipeline

        public T Process<T>(EActionType type, EActionScope scope, Mutator mutator, T current, T source) where T : Data<T> { return current; }

        public KeyValuePair<string, string>? ParseRequest<T>(Dictionary<string, List<string>> requestData) where T: Data<T>
        {
            // Moderation involves no key manipulation.
            return null;
        }

        #endregion
    }
}