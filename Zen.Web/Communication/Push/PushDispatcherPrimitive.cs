using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Zen.Web.Communication.Push
{
    public abstract class PushDispatcherPrimitive
    {
        private readonly Queue<Entry> _messageQueue = new Queue<Entry>();
        private readonly bool _mustCycle = true;

        protected PushDispatcherPrimitive() { Task.Run((Action) DispatcherWorker); }

        private int CycleLengthMilliseconds { get; } = 10000;

        public void Enqueue(EndpointEntry ep, object obj)
        {
            Base.Current.Log.Add("PUSH DispatcherPrimitive: Message enqueued " + ep.endpoint);
            _messageQueue.Enqueue(new Entry {EndpointEntry = ep, Payload = obj});
        }

        private void DispatcherWorker()
        {
            do
            {
                if (_messageQueue.Count == 0)
                {
                    //Task.Delay(CycleLengthMilliseconds); // does not block the thread
                    Thread.Sleep(CycleLengthMilliseconds); // yield the processor and give up your time slice
                }
                else { DispatchQueue(); }
            } while (_mustCycle);
        }

        private void DispatchQueue()
        {
            while (_messageQueue.Count != 0)
            {
                var a = _messageQueue.Peek();

                try { Send(a.EndpointEntry, a.Payload); } catch (Exception e) { Base.Current.Log.Add(e, "DispatchQueue:"); }

                if (_messageQueue.Count != 0) _messageQueue.Dequeue();
            }
        }

        public virtual void Send(EndpointEntry ep, object obj) { }
        public virtual void Register(EndpointEntry ep) { }
        public virtual void Deregister(EndpointEntry ep) { }
        public virtual void HandlePushAttempt(string endpoint, bool success) { }

        private class Entry
        {
            public EndpointEntry EndpointEntry { get; set; }
            public object Payload { get; set; }
        }
    }
}