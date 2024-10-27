using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using Zen.Base.Extension;

namespace Zen.Module.Data.MongoDB.Factories
{
    public class BsonQueryBuilder
    {
        private readonly List<string> _terms = new List<string>();

        public void Add(string term)
        {
            term = term?.Trim();
            if (string.IsNullOrEmpty(term)) return;

            if (term.StartsWith('{') && term.EndsWith('}')) term = term.Substring(1, term.Length - 2); // Remove object delimiters

            _terms.Add(term);
        }

        public BsonDocument Compile()
        {
            var fullBsonQuery = string.Join(",", _terms);

            if (string.IsNullOrEmpty(fullBsonQuery)) return new BsonDocument();

            fullBsonQuery = $"{{{fullBsonQuery}}}";

            // Post-processing: Identify and format ISODates
            var qParts = fullBsonQuery.Split('\"').Where(i => i.Length == 24).ToList();

            if (qParts.Count <= 0) return BsonDocument.Parse(fullBsonQuery);

            foreach (var qPart in qParts)
                if (DateTime.TryParse(qPart, out var dateValue)) // Yay! It's a date
                    fullBsonQuery = fullBsonQuery.Replace("\"" + qPart + "\"", dateValue.ToISODateString());

            return BsonDocument.Parse(fullBsonQuery);
        }
    }
}