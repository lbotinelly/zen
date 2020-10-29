using System;
using System.Linq.Expressions;
using EntityGraphQL.Compiler;
using EntityGraphQL.LinqQuery;
using EntityGraphQL.Schema;

namespace Zen.Web.GraphQL.Common
{
    public class ExpressionFromGraphQlProvider
    {
        private readonly ISchemaProvider _schemaProvider;

        public ExpressionFromGraphQlProvider(ISchemaProvider schemaProvider)
        {
            _schemaProvider = schemaProvider;
        }

        public Expression GetExpression(string graphQl)
        {
            var compiledQueryResult = EntityQueryCompiler.Compile(graphQl, _schemaProvider, null, new DefaultMethodProvider(), null);
            var expressionResult = compiledQueryResult.ExpressionResult;
            var whereMethodExpression = (dynamic) expressionResult.Expression;

            var secondArgument = whereMethodExpression.Arguments[1];

            return (Expression) secondArgument.Operand;
        }
        public Expression<Func<T, bool>> GetExpression<T>(string graphQl)
        {
            var compiledQueryResult = EntityQueryCompiler.Compile(graphQl, _schemaProvider, null, new DefaultMethodProvider(), null);
            var expressionResult = compiledQueryResult.ExpressionResult;
            var whereMethodExpression = (dynamic)expressionResult.Expression;
            return (Expression<Func<T, bool>>)whereMethodExpression.Arguments[1];
        }

    }
}