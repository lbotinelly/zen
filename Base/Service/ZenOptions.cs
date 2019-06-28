namespace Zen.Base.Service
{
    public class ZenOptions
    {
        private ZenOptions value;

        public ZenOptions() { }

        public ZenOptions(ZenOptions value) { this.value = value; }

        public string DefaultScheme { get; set; }
    }
}