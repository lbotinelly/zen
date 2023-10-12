namespace Zen.MessageQueue.Shared
{
    public delegate void MessageReceivedHandler<T>(T item);

    public abstract class MessageQueuePrimitive<T>
    {
        public virtual void Send(T item) { }
        public virtual event MessageReceivedHandler<T> Receive;

    }
}