using System;
using System.Linq;
using MongoDB.Bson;
using MongoDB.Driver;
using Zen.Base.Module;
using Zen.Base.Module.Data;
using Zen.Module.Data.MongoDB.Factories;

namespace Zen.Module.Data.MongoDB
{
    public static class Extensions
    {
        private static readonly char[] FilterFieldDelimiters = {',', '.'};

        public static IFindFluent<BsonDocument, BsonDocument> ApplyTransform<T>(this IMongoCollection<BsonDocument> source, QueryTransform transform) where T : Data<T>
        {
            var queryDocument = transform?.ToBsonQuery() ?? new BsonDocument();
            var sortDocument = transform?.ToBsonFilter() ?? new BsonDocument();

            SortDefinition<BsonDocument> sortFilter = sortDocument;

            if (Info<T>.Configuration?.AutoGenerateMissingSchema == true) source.TryCreateIndex<T>(sortDocument);

            var fluentCollection = source.Find(queryDocument);

            if (sortFilter!= null) fluentCollection.Sort(sortFilter);

            fluentCollection.Paginate(transform);

            return fluentCollection;
        }

        public static IFindFluent<BsonDocument, BsonDocument> Paginate(this IFindFluent<BsonDocument, BsonDocument> source, Pagination pagination)
        {
            source
                .Skip((int) (pagination.Index * pagination.Size))
                .Limit((int) pagination.Size);

            return source;
        }

        public static IFindFluent<BsonDocument, BsonDocument> Paginate(this IFindFluent<BsonDocument, BsonDocument> source, QueryTransform payload)
        {
            if (payload?.Pagination!= null) source.Paginate(payload.Pagination);

            return source;
        }

        public static BsonDocument ToBsonQuery(this QueryTransform modifier)
        {
            var queryBuilder = new BsonQueryBuilder();

            if (modifier == null) return null;

            if (!string.IsNullOrEmpty(modifier.OmniQuery)) queryBuilder.Add($"$text:{{$search: \'{modifier.OmniQuery.Replace("'", "\\'")}\',$caseSensitive: false,$diacriticSensitive: false}}");

            queryBuilder.Add(modifier?.Filter);

            return queryBuilder.Compile();
        }

        public static BsonDocument ToBsonFilter(this QueryTransform modifier)
        {
            var sortFilter = new BsonDocument();

            if (modifier.OrderBy == null) return sortFilter;

            // Valid formats:
            // "Field1,-Field2"
            // "Field1;-Field2"
            // "+ Field1;- Field2"
            // "^Field1"

            var fieldSet = modifier.OrderBy.Split(FilterFieldDelimiters, StringSplitOptions.RemoveEmptyEntries).ToList();

            sortFilter = new BsonDocument();

            foreach (var entry in fieldSet)
            {
                var cleanEntry = entry.Trim();

                var sign = cleanEntry[0]; // Obtain the first character
                var deSignedValue = cleanEntry.Substring(1).Trim();

                int direction;
                string targetProperty;

                switch (sign)
                {
                    // First scenario - user specified a Descending (A-Z, 0-9) direction sign.
                    case '^':
                    case '+':
                        targetProperty = deSignedValue;
                        direction = +1;
                        break;
                    // Second scenario - user specified an Ascending (Z-A, 9-0) direction sign.
                    case '-':
                        targetProperty = deSignedValue;
                        direction = -1;
                        break;
                    // if no sort direction sign is used, assume Descending.
                    default:
                        targetProperty = cleanEntry;
                        direction = +1;
                        break;
                }

                sortFilter[targetProperty] = direction;
            }

            return sortFilter;
        }
    }
}