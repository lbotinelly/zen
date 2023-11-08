using MongoDB.Bson;
using System.Collections.Generic;
using Zen.MessageQueue.Shared;
using Zen.Web.Host;

namespace Sample05_Docker
{
    public class Program
    {
        public static void Main(string[] args)
        {

            Zen.MessageQueue.Queue.RegisterType("string", true);
            Zen.MessageQueue.Queue.RegisterType(new List<string>(), true);

            Zen.MessageQueue.Queue.Receive += (model) =>
            {
                Zen.Base.Log.Add(model.ToJson());
            };

            Zen.MessageQueue.Queue.Send("string", EDistributionStyle.Broadcast);
            Zen.MessageQueue.Queue.Send(new List<string> { "a", "b" }, EDistributionStyle.Broadcast);

            Builder.Start<Startup>(args);


        }
    }
}
