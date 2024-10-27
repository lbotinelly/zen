using System;
using Zen.Base.Common;
using Zen.MessageQueue.Shared;

namespace Zen.Module.MQ.RabbitMQ
{
    [Priority(Level = -2)]
    public class RabbitMQDefaultBundle : IMessageQueueBundle
    {
        public Type AdapterType { get; set; } = typeof(RabbitMQAdapter<>);
    }
}