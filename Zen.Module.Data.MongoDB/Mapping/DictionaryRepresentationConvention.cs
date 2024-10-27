using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Bson.Serialization.Options;

namespace Zen.Module.Data.MongoDB.Mapping
{
    // "BsonSerializationException when serializing a Dictionary<DateTime,T> to BSON"
    // https://stackoverflow.com/a/28111847/1845714
    internal class DictionaryRepresentationConvention : ConventionBase, IMemberMapConvention
    {
        private readonly DictionaryRepresentation _dictionaryRepresentation;

        public DictionaryRepresentationConvention(DictionaryRepresentation dictionaryRepresentation) { _dictionaryRepresentation = dictionaryRepresentation; }

        public void Apply(BsonMemberMap memberMap) { memberMap.SetSerializer(ConfigureSerializer(memberMap.GetSerializer())); }

        private IBsonSerializer ConfigureSerializer(IBsonSerializer serializer)
        {
            if (serializer is IDictionaryRepresentationConfigurable dictionaryRepresentationConfigurable)
                serializer = dictionaryRepresentationConfigurable.WithDictionaryRepresentation(_dictionaryRepresentation);

            return !(serializer is IChildSerializerConfigurable childSerializerConfigurable)
                ? serializer
                : childSerializerConfigurable.WithChildSerializer(ConfigureSerializer(childSerializerConfigurable.ChildSerializer));
        }
    }
}