namespace Zen.MessageQueue.Shared
{
    public delegate void MessageReceivedHandler<T>(T item);

    public enum EDistributionStyle
    {
        RoundRobin,
        Broadcast
    }
    public abstract class MessageQueuePrimitive<T>
    {

        public virtual void Send(T item, EDistributionStyle distributionStyle = EDistributionStyle.Broadcast) { }
        public virtual void Subscribe() { }
        public virtual event MessageReceivedHandler<T> Receive;

    }
}