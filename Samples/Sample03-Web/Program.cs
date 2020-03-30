using Zen.Web.Host;

namespace Sample3_Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Builder.Start<Startup>(args);
        }
    }
}