namespace Zen.MessageQueue
{
    public static class Current
    {
   
        public readonly static Configuration.IOptions Options = Base.Configuration.GetSettings<Configuration.IOptions, Configuration.Options>(new Configuration.Options(), "MessageQueue");
    }
}