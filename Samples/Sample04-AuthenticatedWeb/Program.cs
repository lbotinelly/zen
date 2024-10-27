using Zen.Web.Host;

namespace Sample04_AuthenticatedWeb
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Builder.Start<Startup>(args);
        }
    }
}