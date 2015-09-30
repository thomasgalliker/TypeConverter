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
    }
}