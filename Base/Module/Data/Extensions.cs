using Zen.Base.Module.Data.Adapter;

namespace Zen.Base.Module.Data
{
    public static class Extensions
    {
        //public static Info<T> Info<T>(this Data<T> sourceData) where T : Data<T> => (Info<T>) sourceData;

        public static QueryModifier ToModifier(this string source) => new QueryModifier {Payload = new QueryPayload {FullQuery = source}};
    }
}