using EntityGraphQL.Schema;
using Microsoft.Extensions.Options;
using Zen.Base;
using Zen.Base.Common;
using Zen.Base.Module.Service;
using Zen.Web.GraphQL.Attribute;
using Zen.Web.GraphQL.Common;

namespace Zen.Web.GraphQL
{
    public class GraphQlProcessor : IZenProvider, IGraphQlProcessor
    {
        private static SchemaProvider<object> _schema;
        private readonly Configuration.Options _options;

        public GraphQlProcessor(IOptions<Configuration.Options> options) : this(options.Value) { }

        public GraphQlProcessor(Configuration.Options options) => _options = options;

        public SchemaProvider<object> Schema => _schema;

        public dynamic Context { get; set; } = new { };

        public EOperationalStatus OperationalStatus { get; } = EOperationalStatus.Operational;

        public void Initialize()
        {
            // Load all identified types.

            var queryableTypes = IoC.GetClassesByAttribute<GraphQlAttribute>();

            _schema = SchemaBuilder.FromObject<object>();

            foreach (var (model, attribute) in queryableTypes)
            {
                var name = attribute?.Alias ??
                           (_options.TypeNameResolution == Configuration.ETypeNameResolution.FullName
                               ? model.FullName
                               : model.Name);
                _schema.AddType(model, name, attribute?.Description);
            }

            Current.Log.KeyValuePair(GetType().Name, $"{queryableTypes.Count} queryable types added");
        }

        public string GetState() => "OK";
    }
}