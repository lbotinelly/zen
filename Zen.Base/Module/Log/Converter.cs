using System;
using Zen.Base.Common;
using Zen.Base.Extension;

namespace Zen.Base.Module.Log
{
    public static class Converter
    {
        public static Message ToMessage(string content, Message.EContentType type = Message.EContentType.Generic, string topic = null) { return new Message {Content = content, Subject = type.ToString(), Type = type, Topic = topic}; }

        public static Message ToMessage(Exception e) { return ToMessage(e, null, null); }

        public static Message ToMessage(Exception e, string message, string token = null)
        {
            if (token == null) token = Identifier.MiniGuid();

            message = message == null ? e.ToSummary() : $"{message} ({e.ToSummary()})";

            var ctx = $"{token} : {message}";

            var ret = ToMessage(ctx, Message.EContentType.Exception);

            //try { ret.Topic = HttpContext.Current?.Request?.Url.ToString(); } catch { }

            return ret;
        }

        internal static Message ToMessage<T>(Exception e)
        {
            var tmp = ToMessage(e);
            tmp.Content = $"{typeof(T).Name}: {tmp.Content}";
            return tmp;
        }
    }
}