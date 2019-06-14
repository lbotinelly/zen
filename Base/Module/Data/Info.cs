namespace Zen.Base.Module.Data
{
    public class Info<T> : Data<T> where T : Data<T>
    {
        public string Identifier { get => GetIdentifier(this); set => SetDataIdentifier(value); }
        public string Label { get => GetIdentifier(this); set => SetDataIdentifier(value); }
    }
}