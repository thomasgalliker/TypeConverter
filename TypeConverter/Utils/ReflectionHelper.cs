using System;
using System.Linq.Expressions;
using System.Reflection;

namespace TypeConverter.Utils
{
    internal static class ReflectionHelper
    {
        internal static MethodInfo GetMethod<T>(Expression<Func<T>> expression)
        {
            MethodCallExpression callExpression = (MethodCallExpression)expression.Body;
            return callExpression.Method;
        }

        public static PropertyInfo GetPropertyInfo<TSource, TProperty>(Expression<Func<TSource, TProperty>> expression)
        {
            Type type = typeof(TSource);



            var member = expression.Body as MemberExpression;
            if (member == null)
            {
                if (expression.Body.NodeType == ExpressionType.Convert)
                {
                    var body = (UnaryExpression)expression.Body;
                    member = body.Operand as MemberExpression;
                }

                if (member == null)
                {
                    throw new ArgumentException($"Expression '{expression.ToString()}' refers to a method, not a property.");
                }
            }



            PropertyInfo propInfo = member.Member as PropertyInfo;
            if (propInfo == null)
                throw new ArgumentException($"Expression '{expression.ToString()}' refers to a field, not a property.");

            if (type != propInfo.DeclaringType &&
                !type.GetTypeInfo().IsSubclassOf(propInfo.DeclaringType))
                throw new ArgumentException($"Expresion '{expression.ToString()}' refers to a property that is not from type {type}.");

            return propInfo;
        }
        private static bool IsIndexedPropertyAccess(Expression expression)
        {
            return IsMethodExpression(expression) && expression.ToString().Contains("get_Item");
        }

        private static bool IsMethodExpression(Expression expression)
        {
            return expression is MethodCallExpression || (expression is UnaryExpression && IsMethodExpression((expression as UnaryExpression).Operand));
        }

        ////private static Member GetMember(Expression expression)
        ////{
        ////    if (IsIndexedPropertyAccess(expression))
        ////        return GetDynamicComponentProperty(expression).ToMember();
        ////    if (IsMethodExpression(expression))
        ////        return ((MethodCallExpression)expression).Method.ToMember();

        ////    var memberExpression = GetMemberExpression(expression);

        ////    return memberExpression.Member.ToMember();
        ////}

        ////private static PropertyInfo GetDynamicComponentProperty(Expression expression)
        ////{
        ////    Type desiredConversionType = null;
        ////    MethodCallExpression methodCallExpression = null;
        ////    var nextOperand = expression;

        ////    while (nextOperand != null)
        ////    {
        ////        if (nextOperand.NodeType == ExpressionType.Call)
        ////        {
        ////            methodCallExpression = nextOperand as MethodCallExpression;
        ////            desiredConversionType = desiredConversionType ?? methodCallExpression.Method.ReturnType;
        ////            break;
        ////        }

        ////        if (nextOperand.NodeType != ExpressionType.Convert)
        ////            throw new ArgumentException("Expression not supported", "expression");

        ////        var unaryExpression = (UnaryExpression)nextOperand;
        ////        desiredConversionType = unaryExpression.Type;
        ////        nextOperand = unaryExpression.Operand;
        ////    }

        ////    var constExpression = methodCallExpression.Arguments[0] as ConstantExpression;

        ////    return new DummyPropertyInfo((string)constExpression.Value, desiredConversionType);
        ////}

        public static MemberExpression GetMemberExpression(Expression expression)
        {
            return GetMemberExpression(expression, true);
        }

        public static MemberExpression GetMemberExpression(Expression expression, bool enforceCheck)
        {
            MemberExpression memberExpression = null;
            if (expression.NodeType == ExpressionType.Convert)
            {
                var body = (UnaryExpression)expression;
                memberExpression = body.Operand as MemberExpression;
            }
            else if (expression.NodeType == ExpressionType.MemberAccess)
            {
                memberExpression = expression as MemberExpression;
            }

            if (enforceCheck && memberExpression == null)
            {
                throw new ArgumentException("Not a member access", "expression");
            }

            return memberExpression;
        }
    }
}