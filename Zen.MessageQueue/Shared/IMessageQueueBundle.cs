using System;

namespace Zen.MessageQueue.Shared
{
    public interface IMessageQueueBundle
    {
        Type AdapterType { get; set; }
    }
}
