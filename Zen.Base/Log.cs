using System;
using System.Reflection;
using Zen.Base.Extension;
using Zen.Base.Module.Log;

namespace Zen.Base
{
    public static class Log
    {
        public static void Add(string message, Message.EContentType type = Message.EContentType.Generic, string topic = null)
        {
            if (Status.State != Status.EState.Running)
            {
                Console.WriteLine(topic!= null ? 
                                      $@"{Message.ContentCode[type]} {topic.TruncateEnd(25, true)} : {message.TruncateEnd(53)}" : 
                                      $@"{Message.ContentCode[type]} {message}");
                return;
            }

            Current.Log.Add(message, type, topic);
        }

        public static void Add(string prefix, Exception e) { Add(e, prefix); }

        public static void Add(string pattern, params object[] replacementStrings) { Add(string.Format(pattern, replacementStrings)); }

        public static void Add(Exception[] es)
        {
            foreach (var exception in es) Add(exception, null);
        }

        public static void Add(Exception e) { Add(e, null); }

        public static void Add(Exception e, string message, string token = null)
        {
            if (e is AggregateException es)
            {
                foreach (var e1 in es.InnerExceptions) Add(e1, message, token);
                return;
            }

            if (e.Message.IndexOf("LoaderExceptions", StringComparison.Ordinal) != -1)
            {
                foreach (var exSub in ((ReflectionTypeLoadException)e).LoaderExceptions) Add(message, exSub);
                return;
            }

            message = (message!= null ? message + ": " : "") + e.ToSummary();

            Add(message, Message.EContentType.Exception);

            if (e.InnerException!= null) Add(e.InnerException);
        }

        public static void Add(Type t, string message, Message.EContentType type = Message.EContentType.Generic) { Add(t.FullName + " | " + message, type); }

        public static void Warn<T>(string v)
        {
            KeyValuePair(typeof(T).FullName, v, Message.EContentType.Warning);
        }

        public static void Warn(string content) { Add(content, Message.EContentType.Warning); }

        public static void Add<T>(Exception e)
        {
            Add<T>(e, null);
        }

        public static void Add<T>(string content, Message.EContentType type = Message.EContentType.Info)
        {
            KeyValuePair(typeof(T).FullName, content, type);
        }

        public static void KeyValuePair(string key, string value, Message.EContentType type = Message.EContentType.Info)
        {
            Add(value, type, key);
        }

        public static void Info(string content) { Add(content, Message.EContentType.Info); }

        public static void Info<T>(string content)
        {
            KeyValuePair(typeof(T).FullName, content);
        }

        public static void Debug(string content) { Add(content, Message.EContentType.Debug); }

        public static void Debug<T>(string content)
        {
            KeyValuePair(typeof(T).FullName, content, Message.EContentType.Debug);
        }

        public static void Maintenance(string content) { Add(content, Message.EContentType.Maintenance); }
        public static void Maintenance<T>(string content) { KeyValuePair(typeof(T).FullName, content, Message.EContentType.Maintenance); }

        public static void Startup(string content) { Add(content, Message.EContentType.StartupSequence); }
        public static void Startup<T>(string content) { KeyValuePair(typeof(T).FullName, content, Message.EContentType.StartupSequence); }

        public static void Add<T>(Exception e, string message, string token = null)
        {
            KeyValuePair(typeof(T).FullName, message + " | " + e.ToSummary(), Message.EContentType.Exception);
        }
        private static readonly string _divider = new string('_', 130);
        public static void Divider()
        {
            Debug(_divider);
            Debug("");
        }
    }
}