using System;
using System.Linq.Expressions;
using System.Reflection;

namespace ODataGenerator
{
    public class QueryGenerator<T>
    {
        public string Generate(Expression<Func<T, object>> expression)
        {
            string result = string.Empty;

            return Process(expression.Body);
        }

        private string Process(Expression expression)
        {
            switch (expression.NodeType)
            {
                case ExpressionType.Convert:
                    var unaryExpression = (UnaryExpression) expression;
                    return Process(unaryExpression.Operand);

                case ExpressionType.MemberAccess:
                    var memberExpression = (MemberExpression) expression;
                    var member = memberExpression.Member;
                    if(member is FieldInfo fieldInfo)
                    {
                        object value = null;
                        if(fieldInfo.IsStatic)
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

                    return member.Name;

                case ExpressionType.Equal:
                case ExpressionType.NotEqual:
                case ExpressionType.GreaterThan:
                case ExpressionType.GreaterThanOrEqual:
                case ExpressionType.LessThan:
                case ExpressionType.LessThanOrEqual:
                case ExpressionType.AndAlso:
                case ExpressionType.OrElse:
                    var binaryExpression = (BinaryExpression) expression;
                    var left = WrapIfParenthesisRequired(Process(binaryExpression.Left));
                    var right = WrapIfParenthesisRequired(Process(binaryExpression.Right));
                    var @operator = ResolveOperator(expression.NodeType);
                    return $"{left} " + @operator + $" {right}";

                case ExpressionType.Constant:
                    var valExpression = (ConstantExpression) expression;
                    var val = valExpression.Value;
                    return GetConstantValue(val);
            }

            return string.Empty;
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