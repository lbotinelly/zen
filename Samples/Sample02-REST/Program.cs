using Zen.Web.Host;

namespace Sample02_REST
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Builder.Start<Startup>(args);
        }
    }
}