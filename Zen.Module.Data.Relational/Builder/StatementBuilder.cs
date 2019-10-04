using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using Zen.Base.Extension;
using Zen.Base.Module;

namespace Zen.Module.Data.Relational.Builder
{
    public class StatementBuilder
    {
        private Dictionary<string, RelationalAdapter.MemberDescriptor> _tableDef;
        public StatementMasks Masks = new StatementMasks();

        public WherePart ToSql<T>(Expression<Func<T, bool>> expression, Dictionary<string, RelationalAdapter.MemberDescriptor> descriptor) where T : Data<T>
        {
            _tableDef = descriptor;
            var i = 1;
            return Recurse(ref i, expression.Body, true);
        }

        private WherePart Recurse(ref int i, Expression expression, bool isUnary = false, string prefix = null, string postfix = null)
        {
            switch (expression)
            {
                case UnaryExpression _:
                    var unary = (UnaryExpression) expression;
                    return WherePart.Concat(NodeTypeToString(unary.NodeType), Recurse(ref i, unary.Operand, true));

                case BinaryExpression _:
                    var body = (BinaryExpression) expression;
                    return WherePart.Concat(Recurse(ref i, body.Left), NodeTypeToString(body.NodeType), Recurse(ref i, body.Right));

                case ConstantExpression _:
                    var constant = (ConstantExpression) expression;
                    var value = constant.Value;
                    if (value is int) return WherePart.IsSql(value.ToString());
                    if (value is string) value = prefix + (string) value + postfix;
                    if (value is bool && isUnary) return WherePart.Concat(WherePart.IsParameter(i++, value), "=", WherePart.IsSql("1"));
                    return WherePart.IsParameter(i++, value);

                case MemberExpression _:
                    var member = (MemberExpression) expression;
                    var parametrizedName = "";

                    WherePart response;

                    switch (member.Member)
                    {
                        case PropertyInfo _:
                            var property = (PropertyInfo) member.Member;
                            var colName = _tableDef[property.Name].Field;
                            if (member.Type == typeof(bool))
                            {
                                parametrizedName = Masks.Parameter.format(colName);

                                response = WherePart.IsSql($"{colName} {Masks.Keywords.Equality} {Masks.InlineParameter.format(parametrizedName)}");
                                response.Parameters.Add(parametrizedName, Masks.Values.True);
                            }
                            else if (member.Expression is ConstantExpression)
                            {
                                parametrizedName = Masks.Parameter.format(colName);

                                response = WherePart.IsSql(Masks.InlineParameter.format(parametrizedName));
                                response.Parameters.Add(parametrizedName, GetValue(member));
                            }
                            else
                            {
                                response = WherePart.IsSql(Masks.Column.format(colName));
                            }

                            break;

                        case FieldInfo _:
                            var oValue = GetValue(member);
                            if (oValue is string) oValue = prefix + (string) oValue + postfix;

                            i++;
                            parametrizedName = Masks.Parameter.format(i);

                            response = WherePart.IsSql(Masks.InlineParameter.format(parametrizedName));
                            response.Parameters.Add(parametrizedName, oValue);

                            break;

                        default: throw new Exception($"Expression does not refer to a property or field: {expression}");
                    }

                    return response;

                case MethodCallExpression _:
                    var methodCall = (MethodCallExpression) expression;
                    // LIKE queries:
                    if (methodCall.Method == typeof(string).GetMethod("Contains", new[] {typeof(string)})) return WherePart.Concat(Recurse(ref i, methodCall.Object), "LIKE", Recurse(ref i, methodCall.Arguments[0], prefix: "%", postfix: "%"));
                    if (methodCall.Method == typeof(string).GetMethod("StartsWith", new[] {typeof(string)})) return WherePart.Concat(Recurse(ref i, methodCall.Object), "LIKE", Recurse(ref i, methodCall.Arguments[0], postfix: "%"));
                    if (methodCall.Method == typeof(string).GetMethod("EndsWith", new[] {typeof(string)})) return WherePart.Concat(Recurse(ref i, methodCall.Object), "LIKE", Recurse(ref i, methodCall.Arguments[0], prefix: "%"));
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

                        var values = (IEnumerable) GetValue(collection);
                        return WherePart.Concat(Recurse(ref i, property), "IN", WherePart.IsCollection(ref i, values));
                    }

                    throw new Exception("Unsupported method call: " + methodCall.Method.Name);
            }

            throw new Exception("Unsupported expression: " + expression.GetType().Name);
        }

        private static object GetValue(Expression member)
        {
            // source: http://stackoverflow.com/a/2616980/291955

            return Expression.Lambda(member).Compile().DynamicInvoke();

            var objectMember = Expression.Convert(member, typeof(object));
            var getterLambda = Expression.Lambda<Func<object>>(objectMember);
            var getter = getterLambda.Compile();
            return getter();
        }

        private static string NodeTypeToString(ExpressionType nodeType)
        {
            switch (nodeType)
            {
                case ExpressionType.Add: return "+";
                case ExpressionType.And: return "&";
                case ExpressionType.AndAlso: return "AND";
                case ExpressionType.Divide: return "/";
                case ExpressionType.Equal: return "=";
                case ExpressionType.ExclusiveOr: return "^";
                case ExpressionType.GreaterThan: return ">";
                case ExpressionType.GreaterThanOrEqual: return ">=";
                case ExpressionType.LessThan: return "<";
                case ExpressionType.LessThanOrEqual: return "<=";
                case ExpressionType.Modulo: return "%";
                case ExpressionType.Multiply: return "*";
                case ExpressionType.Negate: return "-";
                case ExpressionType.Not: return "NOT";
                case ExpressionType.NotEqual: return "<>";
                case ExpressionType.Or: return "|";
                case ExpressionType.OrElse: return "OR";
                case ExpressionType.Subtract: return "-";
                case ExpressionType.Convert: return "";
                default: throw new ArgumentOutOfRangeException(nameof(nodeType), nodeType, null);
            }

            throw new Exception($"Unsupported node type: {nodeType}");
        }
    }
}