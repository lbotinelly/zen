using Zen.Web.Host;

namespace Sample05_Docker
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Builder.Start<Startup>(args);
        }
    }
}
