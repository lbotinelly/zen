using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Zen.Base.Common;
using Zen.Base.Extension;

namespace Zen.Base.Module.Log
{
    public abstract class LogProviderBase : ILogProvider
    {
        public static List<Message> Queue = new List<Message>();

        private bool _initialized;
        private Logger _logger;

        public virtual Message.EContentType MaximumLogLevel { get; set; } = Message.EContentType.Debug;

        public virtual void Add(Message message)
        {
            FlushContent(message);

            if (_initialized)
            {
                Pipeline(message);
                return;
            }

            Queue.Add(message);
        }

        public virtual EOperationalStatus OperationalStatus { get; set; } = EOperationalStatus.Undefined;

        public void Initialize()
        {
            Events.StartupSequence.Actions.Add(Start);
            Events.ShutdownSequence.Actions.Add(Shutdown);
        }

        public virtual string GetState() => $"{OperationalStatus}";

        public void FlushQueue()
        {
            foreach (var message in Queue) Pipeline(message);
            Queue.Clear();
        }

        public virtual void BeforePush(Message m) { }

        public virtual void Pipeline(Message m)
        {
            // ReSharper disable once EmptyGeneralCatchClause
            try
            {
                BeforePush(m);
            }
            catch (Exception e)
            {
                Base.Log.Add(e);
            }

            LogEventLevel targetLevel;

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
                case Message.EContentType.Maintenance:
                    targetLevel = LogEventLevel.Information;
                    break;

                case Message.EContentType.Warning:
                case Message.EContentType.Audit:
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

            _logger.Write(targetLevel, (m.Topic != null ? $"{m.Topic} | " : "") + m.Content);
        }

        private static void FlushContent(Message mContent)
        {
            Console.WriteLine(GetThemedContent(mContent));
        }

        private static string GetThemedContent(Message mContent)
        {
            var content = mContent.Topic != null ? $"{mContent.Topic.TruncateEnd(25, true)} { (mContent.Topic.IsNullOrEmpty() ? " " : ":") } {mContent.Content.TruncateEnd(78)}" : mContent.Content;

            if (WindowsConsole.IsAnsi) content = $"{Message.AnsiColors[mContent.Type]}{content}{AnsiTerminalColorCode.ANSI_RESET}";

            return $"{Message.ContentCode[mContent.Type]} {DateTime.Now:HH:mm:ss} {content}";
        }

        public void Start()
        {
            _logger = new LoggerConfiguration()
                .WriteTo.File($"{Host.DataDirectory}{Path.DirectorySeparatorChar}log{Path.DirectorySeparatorChar}log.txt", rollingInterval: RollingInterval.Day)
                .MinimumLevel.Verbose()
                .CreateLogger();

            WindowsConsole.EnableVirtualTerminalProcessing();

            var stdout = Console.OpenStandardOutput();
            Console.SetOut(new StreamWriter(stdout, Encoding.ASCII) { AutoFlush = true });

            _initialized = true;

            FlushQueue();
        }

        public virtual void Shutdown()
        {
            Serilog.Log.CloseAndFlush();
        }

        #region Instanced

        public virtual void Add(bool content)
        {
            Add(content.ToString());
        }

        public virtual void Add(string pattern, params object[] replacementStrings)
        {
            Add(string.Format(pattern, replacementStrings));
        }

        public virtual void Add(Exception[] es)
        {
            foreach (var exception in es) Add(exception, null);
        }

        public virtual void Add(Exception e)
        {
            Add(e, null);
        }

        public virtual void Add(Exception e, string message, string token = null)
        {
            if (e is AggregateException es)
            {
                foreach (var e1 in es.InnerExceptions) Add(e1, message, token);
                return;
            }

            var msg = Converter.ToMessage(e);

            if (message != null) msg.Content = message + " : " + msg.Content;

            Add(msg);

            if (e.InnerException != null) Add(e.InnerException);
        }

        public virtual void Add(Type t, string message, Message.EContentType type = Message.EContentType.Generic)
        {
            Add(t.FullName + " : " + message, type);
        }

        public virtual void Add(string pMessage, Exception e)
        {
            Add(e, pMessage);
        }

        public virtual void Warn(string content, string topic = null)
        {
            Add(content, Message.EContentType.Warning, topic);
        }

        public void KeyValuePair(string key, string value, Message.EContentType type)
        {
            Current.Log.Add(value, type, key);
        }

        public virtual void Info(string content, string topic = null)
        {
            Add(content, Message.EContentType.Info, topic);
        }

        public virtual void Debug(string content, string topic = null)
        {
            Add(content, Message.EContentType.Debug, topic);
        }

        public virtual void Maintenance(string content, string topic = null)
        {
            Add(content, Message.EContentType.Maintenance, topic);
        }

        public virtual void Add(string content, Message.EContentType type = Message.EContentType.Generic, string topic = null)
        {
            if (type > MaximumLogLevel) return; // Ignore all entries over the threshold
            Add(Converter.ToMessage(content, type, topic));
        }

        #endregion

        #region Generics

        public void Warn<T>(string v)
        {
            Warn(v, typeof(T).Name);
        }

        public void Add<T>(Exception e)
        {
            Add<T>(e, null);
        }

        public void Add<T>(string content)
        {
            Add(content, typeof(T).Name);
        }

        public void Info<T>(string content)
        {
            Info(content, typeof(T).Name);
        }

        public void Debug<T>(string content)
        {
            Debug(content, typeof(T).Name);
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

        #endregion
    }
}