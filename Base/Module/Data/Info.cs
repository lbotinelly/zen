namespace Zen.Base.Module.Data
{
    public class Info<T> : Data<T> where T : Data<T>
    {
        public string Identifier { get => GetDataKey(this); set => SetDataKey(value); }
        public string Label { get => GetDataDisplay(this); set => SetDataDisplay(value); }
    }
}