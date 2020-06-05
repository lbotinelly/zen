using System;
using System.Collections;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using Zen.Pebble.Database.Common;

namespace Zen.Pebble.Database
{
    public class ModelRender<T, TFragments, TWherePart> : IModelRender<T, TFragments, TWherePart>
        where TWherePart : IWherePart
        where TFragments : IStatementFragments
    {
        private readonly ModelDescriptor _modelDescriptor;

        public ModelRender(ModelDescriptor modelDefinition = null, StatementMasks masks = null)
        {
            Fragments = (IStatementFragments)typeof(TFragments).CreateInstance();
            WherePart = (IWherePart)typeof(TWherePart).CreateInstance();

            _modelDescriptor = modelDefinition ?? typeof(T).ToModelDescriptor();
            Masks = masks;
        }
        
        public IStatementFragments Fragments { get; set; }
        public IWherePart WherePart { get; set; }

        public StatementMasks Masks { get; set; }

        public IWherePart Render(Expression<Func<T, bool>> expression)
        {
            var i = 0;
            return Recurse(ref i, expression.Body, true);
        }

        private IWherePart Recurse(ref int i, Expression expression, bool isUnary = false, string prefix = null, string postfix = null)
        {
            switch (expression)
            {
                case UnaryExpression unaryExpression:
                    var unary = unaryExpression;
                    return WherePart.Concat(NodeTypeToString(unary.NodeType), Recurse(ref i, unary.Operand, true));

                case BinaryExpression binaryExpression:
                    var body = binaryExpression;
                    return WherePart.Concat(Recurse(ref i, body.Left), NodeTypeToString(body.NodeType), Recurse(ref i, body.Right));

                case ConstantExpression constantExpression:
                    var constant = constantExpression;
                    var value = constant.Value;
                    if (value is int) return WherePart.IsSql(value.ToString());
                    if (value is string) value = prefix + (string)value + postfix;
                    if (value is bool && isUnary) return WherePart.Concat(WherePart.IsParameter(i++, value), "=", WherePart.IsSql("1"));

                    i++;
                    var constantParametrizedName = Fragments.ParametrizedValue.Format(i);

                    var constantResponse = WherePart.IsSql(Fragments.InlineValue.Format(constantParametrizedName));
                    constantResponse.Parameters.Add(constantParametrizedName, value);

                    return constantResponse;
                // return WherePart.IsParameter(i++, value);

                case MemberExpression memberExpression:
                    var member = memberExpression;
                    var parametrizedName = "";

                    IWherePart response = null;

                    var valueSet = GetMemberValueSet(member);

                    switch (member.Member)
                    {
                        case PropertyInfo info:
                            var property = info;
                            var colName = _modelDescriptor.Members[property.Name].TargetName;
                            if (member.Type == typeof(bool))
                            {
                                parametrizedName = Fragments.ParametrizedValue.Format(colName);

                                response = WherePart.IsSql($"{colName} {Fragments.Markers.Equality} {Fragments.InlineValue.Format(parametrizedName)}");
                                response.Parameters.Add(parametrizedName, Fragments.Values.True);
                            }
                            else if (member.Expression is ConstantExpression)
                            {
                                parametrizedName = Fragments.ParametrizedValue.Format(colName);

                                response = WherePart.IsSql(Fragments.InlineValue.Format(parametrizedName));
                                response.Parameters.Add(parametrizedName, GetValue(member));
                            }
                            else
                            {
                                if (valueSet.HasValue) response = WherePart.IsSql(Fragments.Column.Format(colName));
                                else response = WherePart.IsSql(Fragments.Column.Format(colName));
                            }

                            break;

                        case FieldInfo _:

                            if (valueSet.HasValue)
                            {
                                i++;
                                parametrizedName = Fragments.ParametrizedValue.Format(i);

                                response = WherePart.IsSql(Fragments.InlineValue.Format(parametrizedName));
                                response.Parameters.Add(parametrizedName, valueSet.Value);
                            }
                            else { response = WherePart.IsSql(Fragments.Column.Format(valueSet.Member)); }

                            break;

                        default: throw new Exception($"Expression does not refer to a property or field: {expression}");
                    }

                    return response;

                case MethodCallExpression callExpression:
                    var methodCall = callExpression;
                    // LIKE queries:
                    if (methodCall.Method == typeof(string).GetMethod("Contains", new[] { typeof(string) })) return WherePart.Concat(Recurse(ref i, methodCall.Object), "LIKE", Recurse(ref i, methodCall.Arguments[0], prefix: "%", postfix: "%"));
                    if (methodCall.Method == typeof(string).GetMethod("StartsWith", new[] { typeof(string) })) return WherePart.Concat(Recurse(ref i, methodCall.Object), "LIKE", Recurse(ref i, methodCall.Arguments[0], postfix: "%"));
                    if (methodCall.Method == typeof(string).GetMethod("EndsWith", new[] { typeof(string) })) return WherePart.Concat(Recurse(ref i, methodCall.Object), "LIKE", Recurse(ref i, methodCall.Arguments[0], prefix: "%"));
                    // IN queries:
                    if (methodCall.Method.Name == "Contains")
                    {
                        Expression collection;
                        Expression property;
                        if (methodCall.Method.IsDefined(typeof(ExtensionAttribute)) && methodCall.Arguments.Count == 2)
                        {
                            collection = methodCall.Arguments[0];
                            property = methodCall.Arguments[1];
                        }
                        else if (!methodCall.Method.IsDefined(typeof(ExtensionAttribute)) && methodCall.Arguments.Count == 1)
                        {
                            collection = methodCall.Object;
                            property = methodCall.Arguments[0];
                        }
                        else { throw new Exception("Unsupported method call: " + methodCall.Method.Name); }

                        var values = (IEnumerable)GetValue(collection);
                        return WherePart.Concat(Recurse(ref i, property), "IN", WherePart.IsCollection(ref i, values));
                    }

                    throw new Exception("Unsupported method call: " + methodCall.Method.Name);
            }

            throw new Exception("Unsupported expression: " + expression.GetType().Name);
        }

        private MemberValueSet GetMemberValueSet(Expression member)
        {
            // source: http://stackoverflow.com/a/2616980/291955

            var response = new MemberValueSet();
            var lambda = Expression.Lambda(member);

            if (member is MemberExpression expression) response.Member = expression.Member.Name;

            if (_modelDescriptor.Members.ContainsKey(response.Member)) response.Member = _modelDescriptor.Members[response.Member].TargetName;

            try
            {
                var compiled = lambda.Compile();
                response.Value = compiled.DynamicInvoke();
                response.HasValue = true;
            }
            catch (Exception e)
            {
                // This means that this expression needs to be parsed into Container.Member format.
            }

            return response;
        }

        private object GetValue(Expression member)
        {
            // source: http://stackoverflow.com/a/2616980/291955

            var lambda = Expression.Lambda(member);
            object response = null;

            try
            {
                var compiled = lambda.Compile();
                response = compiled.DynamicInvoke();
            }
            catch (Exception e)
            {
                // This means that this expression needs to be parsed into Container.Member format.
            }

            return response;
        }

        private string NodeTypeToString(ExpressionType nodeType)
        {
            switch (nodeType)
            {
                case ExpressionType.Add: return Fragments.NodeAdd;
                case ExpressionType.And: return Fragments.NodeAnd;
                case ExpressionType.AndAlso: return Fragments.NodeAndAlso;
                case ExpressionType.Divide: return Fragments.NodeDivide;
                case ExpressionType.Equal: return Fragments.NodeEqual;
                case ExpressionType.ExclusiveOr: return Fragments.NodeExclusiveOr;
                case ExpressionType.GreaterThan: return Fragments.NodeGreaterThan;
                case ExpressionType.GreaterThanOrEqual: return Fragments.NodeGreaterThanOrEqual;
                case ExpressionType.LessThan: return Fragments.NodeLessThan;
                case ExpressionType.LessThanOrEqual: return Fragments.NodeLessThanOrEqual;
                case ExpressionType.Modulo: return Fragments.NodeModulo;
                case ExpressionType.Multiply: return Fragments.NodeMultiply;
                case ExpressionType.Negate: return Fragments.NodeNegate;
                case ExpressionType.Not: return Fragments.NodeNot;
                case ExpressionType.NotEqual: return Fragments.NodeNotEqual;
                case ExpressionType.Or: return Fragments.NodeOr;
                case ExpressionType.OrElse: return Fragments.NodeOrElse;
                case ExpressionType.Subtract: return Fragments.NodeSubtract;
                case ExpressionType.Convert: return Fragments.NodeConvert;
                default: throw new ArgumentOutOfRangeException(nameof(nodeType), nodeType, null);
            }

            throw new Exception($"Unsupported node type: {nodeType}");
        }

        public class MemberValueSet
        {
            public bool HasValue;
            public object Value;
            public string Member { get; set; }
        }
    }
}