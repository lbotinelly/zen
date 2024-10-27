using EntityGraphQL.Schema;
using Zen.Base.Common;

namespace Zen.Web.GraphQL.Common
{
    public interface IGraphQlProcessor : IZenProvider
    {
        SchemaProvider<object> Schema { get; }
        dynamic Context { get; }
    }
}