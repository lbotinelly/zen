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

        private static readonly Dictionary<Type, object> _cache = new();

        public static event MessageReceivedHandler Receive;

        public static void RegisterType<T>(this T targetObject, bool subscribe = false)
        {
            var adapter = GetAdapter(targetObject);

            if (subscribe)
            {
                adapter.Receive += (item) =>
                {
                    Receive?.Invoke(item);
                };

                adapter.Subscribe();
            }
        }

        public static MessageQueuePrimitive<T> GetAdapter<T>(this T targetObject, bool subscribe = false)
        {

            if (DefaultBundle == null)
                throw new InvalidOperationException("No Message Queue adapter specificed. Try adding a Zen.Module.MQ.* reference.");

            var type = typeof(T);

            if (_cache.ContainsKey(type)) return (MessageQueuePrimitive<T>)_cache[typeof(T)];

            var adapter = DefaultBundle.AdapterType.CreateGenericInstance<T, MessageQueuePrimitive<T>>();

            Base.Log.KeyValuePair("Zen.MessageQueue", $"{typeof(T).Name} adapter - {adapter.GetType().Namespace}", Base.Module.Log.Message.EContentType.Info);

            _cache[typeof(T)] = adapter;

            return adapter;
        }

        public static void Send<T>(this T targetObject, EDistributionStyle distributionStyle = EDistributionStyle.Broadcast)
        {
            GetAdapter(targetObject).Send(targetObject, distributionStyle);
        }
    }
}
