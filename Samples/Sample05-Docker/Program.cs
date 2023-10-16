using Sample05_Docker.Model;
using Zen.Base.Extension;
using Zen.MessageQueue.Shared;
using Zen.Web.Host;

namespace Sample05_Docker
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Zen.MessageQueue.Queue.RegisterType(new TypeA(), true);
            Zen.MessageQueue.Queue.RegisterType(new TypeB());
            Zen.MessageQueue.Queue.RegisterType(new TypeC(), true);

            Zen.MessageQueue.Queue.Receive += model =>
            {
                Zen.Base.Log.Add(model.ToJson());
            };

            Zen.MessageQueue.Queue.Send("string");
            Zen.MessageQueue.Queue.Send(new TypeA());
            Zen.MessageQueue.Queue.Send(new TypeB());
            Zen.MessageQueue.Queue.Send(new TypeC());

            Builder.Start<Startup>(args);


        }
    }
}
