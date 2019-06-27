using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using Newtonsoft.Json.Linq;
// ReSharper disable SpecifyACultureInStringConversionExplicitly

namespace Zen.Module.Data.MongoDB.Serialization
{
    public class JValueSerializer : SerializerBase<JValue>
    {
        public override JValue Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            var myBsonDoc = BsonDocumentSerializer.Instance.Deserialize(context);
            return new JValue(myBsonDoc.ToString());
        }

        public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, JValue value)
        {
            var myBsonDoc = BsonDocument.Parse(value.ToString());
            BsonDocumentSerializer.Instance.Serialize(context, myBsonDoc);
        }
    }
}