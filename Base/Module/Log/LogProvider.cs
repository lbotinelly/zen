using System;
using Microsoft.Extensions.Logging;
using Zen.Base.Common;

namespace Zen.Base.Module.Log
{
    [Priority(Level = -99)]
    public abstract class LogProvider : ILogProvider
    {
        private ILogger<LogProvider> _logger;

        public virtual Message.EContentType MaximumLogLevel { get; set; } = Message.EContentType.Debug;

        public virtual void Add(bool content) { Add(content.ToString()); }

        public virtual void Add(string pattern, params object[] replacementStrings) { Add(string.Format(pattern, replacementStrings)); }

        public virtual void Add(Exception[] es)
        {
            foreach (var exception in es) Add(exception, null, null);
        }

        public virtual void Add(Exception e) { Add(e, null, null); }

        public virtual void Add(Exception e, string message, string token = null)
        {
            if (e is AggregateException es)
            {
                foreach (var e1 in es.InnerExceptions) Add(e1, message, token);
                return;
            }

            Add(Converter.ToMessage(e));
            if (e.InnerException != null) Add(e.InnerException);
        }

        public virtual void Add(Type t, string message, Message.EContentType type = Message.EContentType.Generic) { Add(t.FullName + " : " + message, type); }

        public virtual void Add(string pMessage, Exception e) { Add(e, pMessage, null); }

        public virtual void Warn(string content) { Add(content, Message.EContentType.Warning); }

        public virtual void Info(string content) { Add(content, Message.EContentType.Info); }

        public virtual void Maintenance(string content) { Add(content, Message.EContentType.Maintenance); }

        public virtual void Add(string content, Message.EContentType type = Message.EContentType.Generic)
        {
            if (type > MaximumLogLevel) return; // Ignore all entries over the threshold

            var payload = Converter.ToMessage(content, type);

            Add(payload);
        }

        public virtual void Add(Message m)
        {
            var targetLevel = LogLevel.None;

            switch (m.Type)
            {
                case Message.EContentType.Undefined:
                case Message.EContentType.Generic:
                    targetLevel = LogLevel.Trace;
                    break;
                case Message.EContentType.Debug:
                    targetLevel = LogLevel.Debug;
                    break;

                case Message.EContentType.StartupSequence:
                case Message.EContentType.ShutdownSequence:
                case Message.EContentType.Info:
                case Message.EContentType.MoreInfo:
                    targetLevel = LogLevel.Information;
                    break;

                case Message.EContentType.Warning:
                case Message.EContentType.Audit:
                case Message.EContentType.Maintenance:
                    targetLevel = LogLevel.Information;
                    break;

                case Message.EContentType.Exception:

                    targetLevel = LogLevel.Error;
                    break;
                default: throw new ArgumentOutOfRangeException();
            }

            _logger.Log(targetLevel, m.Content, m);
        }

        public void Initialize()
        {
            Events.Bootup.Actions.Add(Start);
            Events.Shutdown.Actions.Add(Shutdown);
        }

        public void Start() { _logger = ((ILoggerFactory) Current.ServiceProvider.GetService(typeof(ILoggerFactory))).CreateLogger<LogProvider>(); }

        public virtual void Shutdown() { }
    }
}