namespace Zen.Base.Module.Data
{
    public static class Extensions
    {
        public static Info<T> Info<T>(this Data<T> sourceData) where T : Data<T> { return (Info<T>) sourceData; }
    }
}