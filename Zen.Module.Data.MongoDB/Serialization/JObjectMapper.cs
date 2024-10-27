using MongoDB.Bson;
using Newtonsoft.Json.Linq;

namespace Zen.Module.Data.MongoDB.Serialization
{
    public class JObjectMapper : ICustomBsonTypeMapper
    {
        public bool TryMapToBsonValue(object value, out BsonValue bsonValue)
        {
            bsonValue = value is JObject src ? BsonDocument.Parse("{_p:" + src + "}")[0] : BsonValue.Create(value);
            return true;
        }
    }
}