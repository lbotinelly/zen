namespace Zen.Base.Module.Environment
{
    public sealed class DefaultEnvironmentDescriptor : IEnvironmentDescriptor
    {
        //The default Descriptor handles only one environment.
        public static readonly IEnvironmentDescriptor Standard = new DefaultEnvironmentDescriptor(0, "STA", "Standard");

        private DefaultEnvironmentDescriptor(int value, string code, string name)
        {
            CacheDatabaseIndex = value;
            Name = name;
            Code = code;
        }

        public string Name { get; }
        public string Code { get; }
        public int CacheDatabaseIndex { get; }

        public override string ToString() { return Name; }
    }
}