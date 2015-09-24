using System;
using System.Linq.Expressions;
using System.Reflection;

using Guards;

namespace TypeConverter.Utils
{
    /// <summary>
    ///     This class allows you to get members from types more safely than using
    ///     string literals. It only exists because C# does not have fieldinfoof,
    ///     propertyinfoof and methodinfoof.
    /// </summary>
    public static class ReflectionHelper
    {
        /// <summary>
        ///     Gets a member by it's expression usage.
        ///     For example, GetMember(() => obj.GetType()) will return the
        ///     GetType method.
        /// </summary>
        public static MemberInfo GetMember<T>(Expression<Func<T>> expression)
        {
           Guard.ArgumentNotNull(() => expression);

            var body = expression.Body;

            switch (body.NodeType)
            {
                case ExpressionType.MemberAccess:
                    MemberExpression memberExpression = (MemberExpression)body;
                    return memberExpression.Member;

                case ExpressionType.Call:
                    MethodCallExpression callExpression = (MethodCallExpression)body;
                    return callExpression.Method;

                case ExpressionType.New:
                    NewExpression newExpression = (NewExpression)body;
                    return newExpression.Constructor;
            }

            throw new ArgumentException("expression.Body must be a member or call expression.", "expression");
        }

        /// <summary>
        ///     Gets a method info of a void method.
        ///     Example: GetMethod(() => Console.WriteLine("")); will return the
        ///     MethodInfo of WriteLine that receives a single argument.
        /// </summary>
        public static MethodInfo GetMethod(Expression<Action> expression)
        {
            if (expression == null)
            {
                throw new ArgumentNullException("expression");
            }

            var body = expression.Body;
            if (body.NodeType != ExpressionType.Call)
            {
                throw new ArgumentException("expression.Body must be a Call expression.", "expression");
            }

            MethodCallExpression callExpression = (MethodCallExpression)body;
            return callExpression.Method;
        }

        /// <summary>
        ///     Gets the MethodInfo of a method that returns a value.
        ///     Example: GetMethod(() => Console.ReadLine()); will return the method info
        ///     of ReadLine.
        /// </summary>
        public static MethodInfo GetMethod<T>(Expression<Func<T>> expression)
        {
            return (MethodInfo)GetMember(expression);
        }
    }
}