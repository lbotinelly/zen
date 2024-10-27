using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using Newtonsoft.Json.Linq;
// ReSharper disable SpecifyACultureInStringConversionExplicitly

namespace Zen.Module.Data.MongoDB.Serialization
{
    public class JObjectSerializer : SerializerBase<JObject>
    {
        public override JObject Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            var myBsonDoc = BsonDocumentSerializer.Instance.Deserialize(context);
            return JObject.Parse(myBsonDoc.ToString());
        }

        public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, JObject value)
        {
            var myBsonDoc = BsonDocument.Parse(value.ToString());
            BsonDocumentSerializer.Instance.Serialize(context, myBsonDoc);
        }
    }
}