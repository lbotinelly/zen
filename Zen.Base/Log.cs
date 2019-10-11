﻿using System;
using System.Reflection;
using Zen.Base.Extension;
using Zen.Base.Module.Log;

namespace Zen.Base
{
    public static class Log
    {
        public static void Add(string message, Message.EContentType type = Message.EContentType.Generic)
        {
            if (Status.State != Status.EState.Running)
            {
                Console.WriteLine($"{Message.ContentCode[type]} {message}");
                return;
            }

            Current.Log.Add(message, type);
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
                foreach (var exSub in ((ReflectionTypeLoadException) e).LoaderExceptions) Add(message, exSub);
                return;
            }

            message = (message != null ? message + ": " : "") + e.ToSummary();

            Add(message, Message.EContentType.Exception);

            if (e.InnerException != null) Add(e.InnerException);
        }

        public static void Add(Type t, string message, Message.EContentType type = Message.EContentType.Generic) { Add(t.FullName + " | " + message, type); }

        public static void Warn<T>(string v) { Warn(typeof(T).Name + ": " + v); }

        public static void Warn(string content) { Add(content, Message.EContentType.Warning); }

        public static void Add<T>(Exception e) { Add<T>(e, null); }
        public static void Add<T>(string content) { Add(typeof(T).Name + ": " + content); }

        public static void KeyValuePair(string key, string value, Message.EContentType type = Message.EContentType.Info) { Add($"{key.TruncateEnd(35, true)} : {value.TruncateEnd(93)}", type); }

        public static void Info(string content) { Add(content, Message.EContentType.Info); }

        public static void Info<T>(string content) { Info(typeof(T).Name + ": " + content); }

        public static void Debug(string content) { Add(content, Message.EContentType.Debug); }

        public static void Debug<T>(string content) { Debug(typeof(T).Name + ": " + content); }

        public static void Maintenance(string content) { Add(content, Message.EContentType.Maintenance); }

        public static void Add<T>(Exception e, string message, string token = null)
        {
            message = typeof(T).Name + ": " + message;

            Add(message, Message.EContentType.Exception);
        }
    }
}