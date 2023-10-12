using System;
using System.Collections.Generic;
using System.Linq;
using Zen.Base.Extension;
using Zen.Base.Module.Service;
using Zen.MessageQueue.Shared;

namespace Zen.MessageQueue
{

    public delegate void MessageReceivedHandler(object item);

    public static class Queue
    {
        private static readonly IMessageQueueBundle DefaultBundle = (IMessageQueueBundle)IoC.GetClassesByInterface<IMessageQueueBundle>(false)?.First()?.CreateInstance();

        private static readonly Dictionary<Type, object> _cache = new Dictionary<Type, object>();

        public static event MessageReceivedHandler Receive;


        public static void RegisterType<T>(this T targetObject)
        {

            if (DefaultBundle == null)
                throw new InvalidOperationException("No Message Queue adapter specificed. Try adding a Zen.Module.MQ.* reference.");

            var type = typeof(T);

            if (_cache.ContainsKey(type)) return;
            MessageQueuePrimitive<T> adapter = DefaultBundle.AdapterType.CreateGenericInstance<T, MessageQueuePrimitive<T>>();

            _cache[typeof(T)] = adapter;

            adapter.Receive += (item) =>
            {
                Receive?.Invoke(item);
            };
        }

        public static void Send<T>(this T targetObject)
        {
            ((MessageQueuePrimitive<T>)_cache[typeof(T)]).Send(targetObject);
        }
    }
}
