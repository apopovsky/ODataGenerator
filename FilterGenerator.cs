using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace ODataGenerator
{
    public class FilterGenerator<T>
    {
        private readonly string[] _supportedMethods = { "all", "any" };

        public string Generate(Expression<Func<T, object>> expression)
        {
            return Process(expression.Body, null);
        }

        private string Process(Expression expression, string alias)
        {
            switch (expression.NodeType)
            {
                case ExpressionType.Convert:
                    var unaryExpression = (UnaryExpression) expression;
                    return Process(unaryExpression.Operand, alias);

                case ExpressionType.MemberAccess:
                    var memberExpression = (MemberExpression) expression;
                    var member = memberExpression.Member;
                    if (memberExpression.Expression ==null ||
                        memberExpression.Expression.NodeType == ExpressionType.MemberAccess ||
                        memberExpression.Expression.NodeType == ExpressionType.Constant)
                    {
                        return GetMemberValue(member, memberExpression);
                    }

                    return !string.IsNullOrEmpty(alias) ? $"{alias}/{member.Name}" : member.Name;

                case ExpressionType.Equal:
                case ExpressionType.NotEqual:
                case ExpressionType.GreaterThan:
                case ExpressionType.GreaterThanOrEqual:
                case ExpressionType.LessThan:
                case ExpressionType.LessThanOrEqual:
                case ExpressionType.AndAlso:
                case ExpressionType.OrElse:
                    var binaryExpression = (BinaryExpression) expression;
                    var left = WrapIfParenthesisRequired(Process(binaryExpression.Left, alias));
                    var right = WrapIfParenthesisRequired(Process(binaryExpression.Right, alias));
                    var @operator = ResolveOperator(expression.NodeType);
                    return $"{left} " + @operator + $" {right}";

                case ExpressionType.Constant:
                    var valExpression = (ConstantExpression) expression;
                    var val = valExpression.Value;
                    return GetConstantValue(val);
                case ExpressionType.Call:
                    var methodCallExpression = (MethodCallExpression) expression;
                    var property = Process(methodCallExpression.Arguments[0], alias);
                    var innerLambda = (LambdaExpression) methodCallExpression.Arguments[1];
                    var innerAlias = innerLambda.Parameters[0].Name;
                    var filter = Process(innerLambda.Body, innerAlias);
                    var methodName = methodCallExpression.Method.Name.ToLower();
                    if(!_supportedMethods.Contains(methodName)) throw new NotImplementedException();
                    return $"{property}/{methodName}({innerAlias}:{filter})";
                default:
                    throw new NotImplementedException();
            }
        }

        private static string GetMemberValue(MemberInfo member, MemberExpression memberExpression)
        {
            switch (member)
            {
                case FieldInfo fieldInfo:
                {
                    object value;
                    if (fieldInfo.IsStatic)
                    {
                        value = fieldInfo.GetValue(null);
                    }
                    else
                    {
                        var f = Expression.Lambda(memberExpression).Compile();
                        value = f.DynamicInvoke();
                    }

                    return GetConstantValue(value);
                }
                case PropertyInfo propertyInfo:
                {
                    object value;
                    if (propertyInfo.GetGetMethod().IsStatic)
                    {
                        value = propertyInfo.GetValue(null);
                    }
                    else
                    {
                        var f = Expression.Lambda(memberExpression).Compile();
                        value = f.DynamicInvoke();
                    }

                    return GetConstantValue(value);
                }
                default:
                    throw new NotImplementedException();
            }
        }

        private static string WrapIfParenthesisRequired(string terms)
        {
            if (terms.Contains(" and ") || terms.Contains(" or ")) terms = $"({terms})";
            return terms;
        }

        private static string GetConstantValue(object constantValue)
        {
            if (constantValue == null) return "null";
            var value = constantValue.ToString();
            return constantValue.GetType().IsPrimitive ? value : $"'{constantValue}'";
        }

        private string ResolveOperator(ExpressionType nodeType)
        {
            switch (nodeType)
            {
                case ExpressionType.Equal:
                    return "eq";
                case ExpressionType.NotEqual:
                    return "ne";
                case ExpressionType.GreaterThan:
                    return "gt";
                case ExpressionType.GreaterThanOrEqual:
                    return "ge";
                case ExpressionType.LessThan:
                    return "le";
                case ExpressionType.LessThanOrEqual:
                    return "lt";
                case ExpressionType.AndAlso:
                    return "and";
                case ExpressionType.OrElse:
                    return "or";
                case ExpressionType.Negate:
                    return "not";
                default:
                    throw new NotImplementedException();
            }
        }
    }
}