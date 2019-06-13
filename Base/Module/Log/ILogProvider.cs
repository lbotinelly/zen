using System;
using Zen.Base.Common;

namespace Zen.Base.Module.Log
{
    public interface ILogProvider: IZenProvider
    {
        Message.EContentType MaximumLogLevel { get; set; }
        void Add(bool content);
        void Add(Exception e);
        void Add(Exception e, string message, string token = null);
        void Add(Exception[] es);
        void Add(Message m);
        void Add(string content, Message.EContentType type = Message.EContentType.Generic);
        void Add(string pMessage, Exception e);
        void Add(string pattern, params object[] replacementStrings);
        void Add(Type t, string message, Message.EContentType type = Message.EContentType.Generic);
        void Info(string content);
        void Maintenance(string content);
        void Warn(string content);
    }
}