using Zen.Base.Module;
using Zen.Base.Module.Data;

namespace Zen.Module.Data.MongoDB
{
    public class StaticLock<T> where T : Data<T>
    {
        public static object Lock { get; set; } = new object();
    }
}
