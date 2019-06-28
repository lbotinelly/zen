using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using Serilog;
using Serilog.Core;
using Serilog.Events;

namespace Zen.Base.Module.Log
{
    public abstract class LogProvider : ILogProvider
    {
        private Logger _logger;

        protected LogProvider() { }

        public virtual Message.EContentType MaximumLogLevel { get; set; } = Message.EContentType.Debug;

        public virtual void Add(bool content) { Add(content.ToString()); }

        public virtual void Add(string pattern, params object[] replacementStrings) { Add(string.Format(pattern, replacementStrings)); }

        public virtual void Add(Exception[] es)
        {
            foreach (var exception in es) Add(exception, null);
        }

        public virtual void Add(Exception e) { Add(e, null); }

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

        public virtual void Add(string pMessage, Exception e) { Add(e, pMessage); }

        public void Warn<T>(string v) { Warn(typeof(T).Name + ": " + v); }

        public virtual void Warn(string content) { Add(content, Message.EContentType.Warning); }

        public void Add<T>(Exception e) { Add<T>(e, null); }

        public virtual void Info(string content) { Add(content, Message.EContentType.Info); }

        public virtual void Debug(string content) { Add(content, Message.EContentType.Debug); }

        public void Debug<T>(string content) { Debug(typeof(T).Name + ": " + content); }

        public virtual void Maintenance(string content) { Add(content, Message.EContentType.Maintenance); }

        public virtual void Add(string content, Message.EContentType type = Message.EContentType.Generic)
        {
            if (type > MaximumLogLevel) return; // Ignore all entries over the threshold

            var payload = Converter.ToMessage(content, type);

            Add(payload);
        }

        public static List<Message> _queue = new List<Message>();

        public void FlushQueue()
        {
            foreach (var message in _queue)
            {
                Pipeline(message);
            }

            _queue.Clear();
        }

        public virtual void Add(Message message)
        {
            if (_logger != null)
            {
                Pipeline(message);
                return;
            }

            _queue.Add(message);
        }

        public virtual void Pipeline(Message m)
        {


            var targetLevel = LogEventLevel.Debug;

            switch (m.Type)
            {
                case Message.EContentType.Undefined:
                case Message.EContentType.Generic:
                    targetLevel = LogEventLevel.Verbose;
                    break;

                case Message.EContentType.Debug:
                    targetLevel = LogEventLevel.Debug;
                    break;

                case Message.EContentType.StartupSequence:
                case Message.EContentType.ShutdownSequence:
                case Message.EContentType.Info:
                case Message.EContentType.MoreInfo:
                    targetLevel = LogEventLevel.Information;
                    break;

                case Message.EContentType.Warning:
                case Message.EContentType.Audit:
                case Message.EContentType.Maintenance:
                    targetLevel = LogEventLevel.Warning;
                    break;

                case Message.EContentType.Exception:
                    targetLevel = LogEventLevel.Error;
                    break;

                case Message.EContentType.Critical:
                    targetLevel = LogEventLevel.Fatal;
                    break;

                default: throw new ArgumentOutOfRangeException();
            }

            _logger.Write(targetLevel, m.Content, m);

            //_logger.Log(targetLevel, m.Content, m);
        }


        public void Initialize()
        {
            Events.StartupSequence.Actions.Add(Start);
            Events.ShutdownSequence.Actions.Add(Shutdown);
        }

        public virtual void Add<T>(Exception e, string message, string token = null)
        {
            if (e is AggregateException es)
            {
                foreach (var e1 in es.InnerExceptions) Add<T>(e1, message, token);
                return;
            }

            Add(Converter.ToMessage<T>(e));

            if (e.InnerException != null) Add<T>(e.InnerException);
        }

        public void Start()
        {
            //_logger = ((ILoggerFactory) Current.ServiceProvider.GetService(typeof(ILoggerFactory))).CreateLogger<LogProvider>();
            _logger = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .WriteTo.File("log.txt", rollingInterval: RollingInterval.Day)
                .MinimumLevel.Debug()
                .CreateLogger();

            FlushQueue();
        }

        public virtual void Shutdown() { Serilog.Log.CloseAndFlush(); }
    }
}