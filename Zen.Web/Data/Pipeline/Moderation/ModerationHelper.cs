using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Zen.Base.Extension;
using Zen.Base.Module;
using Zen.Base.Module.Data;
using Zen.Web.Data.Pipeline.Moderation.Shared;

namespace Zen.Web.Data.Pipeline.Moderation
{
    public static class ModerationHelper
    {
        private static readonly Dictionary<Type, object> ConfigBlockCache = new Dictionary<Type, object>();
        private static readonly Dictionary<Type, ICustomModerationPipeline> CustomModerationPipelineCache = new Dictionary<Type, ICustomModerationPipeline>();
        private static readonly object ConcurrentLock = new object();

        public static ConfigBlock<T> Setup<T>() where T : Data<T>
        {
            lock (ConcurrentLock)
            {
                var t = typeof(T);
                if (ConfigBlockCache.ContainsKey(t)) return (ConfigBlock<T>)ConfigBlockCache[t];

                var e = new ConfigBlock<T>
                {
                    Setup = typeof(T).GetCustomAttributes(false).OfType<ModerationPrimitiveAttribute>().FirstOrDefault(),
                    LogManager = new ModerationLog<T>(),
                    TaskManager = new ModerationTask<T>()
                };

                ConfigBlockCache.Add(t, e);

                ConfigBlockCache[t] = e;

                return e;
            }
        }

        public static ModerationControlPayload GetModerationControlPayload()
        {
            if (Current.Context == null) return null;

            var response = new ModerationControlPayload();

            try
            {
                // Maybe it came through POST:
                response = Current.Context.Request.BodyContent().FromJson<ModerationControlPayload>();
                if (response!= null) return response;
            }
            catch { }

            // But Querystring is also an option.

            if (!Current.Context.Request.Query.ContainsKey("___ModerationTaskId")) return null;

            try { response.___ModerationTaskId = Current.Context.Request.Query["___ModerationTaskId"]; } catch { }
            try { response.___ModerationRationale = Current.Context.Request.Query["___ModerationRationale"]; } catch { }
            return response;
        }

        public class ConfigBlock<T> where T : Data<T>
        {
            public ModerationPrimitiveAttribute Setup { get; set; }
            public ModerationTask<T> TaskManager { get; set; }
            public ModerationLog<T> LogManager { get; set; }

            public ICustomModerationPipeline CustomModerationPipeline
            {
                get
                {
                    lock (ConcurrentLock)
                    {
                        var targetType = typeof(T);
                        if (CustomModerationPipelineCache.ContainsKey(targetType)) return CustomModerationPipelineCache[targetType];

                        var targetModerationPipeline = typeof(ICustomModerationPipeline).IsAssignableFrom(typeof(T)) ? (ICustomModerationPipeline)Info<T>.Instance : null;

                        CustomModerationPipelineCache.Add(targetType, targetModerationPipeline);

                        return targetModerationPipeline;
                    }
                }
            }

            internal States.ModerationActions ModerationActions
            {
                get
                {
                    var response = new States.ModerationActions
                    {
                        Moderate = Setup.ModeratorPermission!= null && App.Current.Orchestrator.Person?.HasAnyPermissions(Setup.ModeratorPermission) == true,
                        Whitelisted = Setup.WhitelistPermission!= null && App.Current.Orchestrator.Person?.HasAnyPermissions(Setup.WhitelistPermission) == true,
                        Author = App.Current.Orchestrator.Person?.HasAnyPermissions(Setup.CreatorPermission) == true
                    };

                    response.Read = response.Moderate || response.Whitelisted || response.Author;

                    return response;
                }
            }
        }
    }
}