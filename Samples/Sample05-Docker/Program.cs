using MongoDB.Bson;
using Zen.Web.Host;

namespace Sample05_Docker
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Zen.MessageQueue.Queue.RegisterType("string");
            Zen.MessageQueue.Queue.Send("string");
            Zen.MessageQueue.Queue.Receive += (model) =>
            {
                Zen.Base.Log.Add(model.ToJson());
            };


            Builder.Start<Startup>(args);


        }
    }
}
