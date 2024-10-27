using System;
using System.Net;
using EntityGraphQL;
using Microsoft.AspNetCore.Mvc;
using Zen.Base;
using Zen.Web.GraphQL.Common;

namespace Zen.Web.GraphQL.Query
{
    [Route("[controller]")]
    public class GraphQlController : ControllerBase
    {
        // private static readonly ConcurrentDictionary<Type, ExpressionFromGraphQlProvider> ExpressionProviderCache = new ConcurrentDictionary<Type, ExpressionFromGraphQlProvider>();
        private readonly IGraphQlProcessor _graphQlProcessor;

        public GraphQlController(IGraphQlProcessor graphQlProcessor)
        {
            _graphQlProcessor = graphQlProcessor;
        }

        //private ExpressionFromGraphQlProvider ExpressionProvider
        //{
        //    get
        //    {
        //        if (ExpressionProviderCache.ContainsKey(typeof(T))) return ExpressionProviderCache[typeof(T)];

        //        var expressionFromGraphQlProvider = new ExpressionFromGraphQlProvider(SchemaBuilder.FromObject<T>());

        //        ExpressionProviderCache[typeof(T)] = expressionFromGraphQlProvider;

        //        return expressionFromGraphQlProvider;
        //    }
        //}


        [HttpPost("query")]
        public object Post([FromBody] QueryRequest query)
        {
            try
            {
                Current.Log.KeyValuePair(GetType().Name, $"{query.OperationName}");

                var results = _graphQlProcessor.Schema.ExecuteQuery(query, _graphQlProcessor.Context, null, null);

                // var expression = ExpressionProvider.GetExpression<T>(query.Query);
                // gql compile errors show up in results.Errors
                return results;
            }
            catch (Exception e)
            {
                Log.Add(e);
                return HttpStatusCode.InternalServerError;
            }
        }
    }
}