namespace Zen.Base.Module.Data
{
    public class StaticLock<T> where T : Data<T>
    {
        public static object Lock { get; set; } = new object();
    }
}