using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

using LockIntegralType = System.Int32;

namespace TypeConverter
{
    /// <summary>
    ///     Delegate that represents a dynamic-call to an untyped delegate.
    ///     It is faster than simple calling DynamicInvoke.
    /// </summary>
    public delegate object FastDynamicDelegate(params object[] parameters);

    /// <summary>
    ///     Delegate that represents a dynamic-call to a methodInfo.
    ///     It is faster than calling the methodInfo.Invoke.
    /// </summary>
    public delegate object FastMethodCallDelegate(object target, params object[] parameters);

    /// <summary>
    ///     This class allows you to get members from types more safely than using
    ///     string literals. It only exists because C# does not have fieldinfoof,
    ///     propertyinfoof and methodinfoof.
    /// </summary>
    public static class ReflectionHelper
    {
        #region GetMember

        /// <summary>
        ///     Gets a member by it's expression usage.
        ///     For example, GetMember(() => obj.GetType()) will return the
        ///     GetType method.
        /// </summary>
        public static MemberInfo GetMember<T>(Expression<Func<T>> expression)
        {
            if (expression == null)
            {
                throw new ArgumentNullException("expression");
            }

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

        #endregion

        #region GetConstructor

        /// <summary>
        ///     Gets the constructor info from a sample construction call expression.
        ///     Example: GetConstructor(() => new Control()) will return the constructor
        ///     info for the default constructor of Control.
        /// </summary>
        public static ConstructorInfo GetConstructor<T>(Expression<Func<T>> expression)
        {
            return (ConstructorInfo)GetMember(expression);
        }

        #endregion

        #region GetField

        /// <summary>
        ///     Gets a field from a sample usage.
        ///     Example: GetField(() => Type.EmptyTypes) will return the FieldInfo of
        ///     EmptyTypes.
        /// </summary>
        public static FieldInfo GetField<T>(Expression<Func<T>> expression)
        {
            return (FieldInfo)GetMember(expression);
        }

        #endregion

        #region GetProperty

        /// <summary>
        ///     Gets a property from a sample usage.
        ///     Example: GetProperty(() => str.Length) will return the property info
        ///     of Length.
        /// </summary>
        public static PropertyInfo GetProperty<T>(Expression<Func<T>> expression)
        {
            return (PropertyInfo)GetMember(expression);
        }

        #endregion

        #region GetMethod

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

        #endregion

        #region GetDefaultConstructorDelegate

        private static YieldReaderWriterLockSlim _defaultConstructorsLock;
        private static readonly Dictionary<Type, Func<object>> _defaultConstructorsDictionary = new Dictionary<Type, Func<object>>();
        private static readonly Func<Type, Func<object>> _getDefaultConstructorDelegate = GetDefaultConstructorDelegate<object>;

        /// <summary>
        ///     Gets a function that creates objects of the given type.
        ///     The object must have a default constructor.
        /// </summary>
        public static Func<object> GetDefaultConstructorDelegate(Type objectType)
        {
            var result = Ext.GetOrCreateValue(_defaultConstructorsDictionary, ref _defaultConstructorsLock, objectType, _getDefaultConstructorDelegate);
            return result;
        }

        private static YieldReaderWriterLockSlim _typedDefaultConstructorsLock;
        private static readonly Dictionary<KeyValuePair<Type, Type>, Delegate> _typedDefaultConstructorsDictionary = new Dictionary<KeyValuePair<Type, Type>, Delegate>();
        private static readonly Func<KeyValuePair<Type, Type>, Delegate> _getTypedDefaultConstructorDelegate = _GetDefaultConstructorDelegate;

        /// <summary>
        ///     Gets the default constructor for the given objectType, but return
        ///     it already casted to a given "T".
        /// </summary>
        public static Func<T> GetDefaultConstructorDelegate<T>(Type objectType)
        {
            if (objectType == null)
            {
                throw new ArgumentNullException("objectType");
            }

            var pair = new KeyValuePair<Type, Type>(typeof(Func<T>), objectType);
            var result = Ext.GetOrCreateValue(_typedDefaultConstructorsDictionary, ref _typedDefaultConstructorsLock, pair, _getTypedDefaultConstructorDelegate);
            return (Func<T>)result;
        }

        private static Delegate _GetDefaultConstructorDelegate(KeyValuePair<Type, Type> pair)
        {
            var funcType = pair.Key;
            var objectType = pair.Value;
            var resultType = funcType.GetTypeInfo().GenericTypeArguments[0];

            Expression expression = Expression.New(objectType);

            if (resultType != objectType && (objectType.GetTypeInfo().IsValueType || !resultType.GetTypeInfo().IsAssignableFrom(objectType.GetTypeInfo())))
            {
                expression = Expression.Convert(expression, resultType);
            }

            var lambdaExpression = Expression.Lambda(funcType, expression);
            var func = lambdaExpression.Compile();

            return func;
        }

        #endregion

        #region GetConstructorDelegate<T>

        /// <summary>
        ///     Creates a delegate (of type T) for the given constructor.
        ///     The delegate type should match the number of parameters in the constructor.
        ///     Casts are done if required but no other conversions are done.
        /// </summary>
        public static T GetConstructorDelegate<T>(ConstructorInfo constructor)
        {
            object result = GetConstructorDelegate(constructor, typeof(T));
            return (T)result;
        }

        private static YieldReaderWriterLockSlim _getTypedConstructorLock;
        private static readonly Dictionary<KeyValuePair<ConstructorInfo, Type>, Delegate> _typedConstructors = new Dictionary<KeyValuePair<ConstructorInfo, Type>, Delegate>();
        private static readonly Func<KeyValuePair<ConstructorInfo, Type>, Delegate> _getTypedConstructorDelegate = _GetConstructorDelegate;

        /// <summary>
        ///     Creates a delegate for the given constructor.
        ///     The delegate type should match the number of parameters in the constructor.
        ///     Casts are done if required but no other conversions are done.
        /// </summary>
        public static Delegate GetConstructorDelegate(ConstructorInfo constructor, Type delegateType)
        {
            if (constructor == null)
            {
                throw new ArgumentNullException("constructor");
            }

            if (!delegateType.GetTypeInfo().IsSubclassOf(typeof(Delegate)))
            {
                throw new ArgumentException("delegateType is not a Delegate.", "delegateType");
            }

            var pair = new KeyValuePair<ConstructorInfo, Type>(constructor, delegateType);
            var result = Ext.GetOrCreateValue(_typedConstructors, ref _getTypedConstructorLock, pair, _getTypedConstructorDelegate);
            return result;
        }

        private static Delegate _GetConstructorDelegate(KeyValuePair<ConstructorInfo, Type> pair)
        {
            var constructor = pair.Key;
            var delegateType = pair.Value;

            var invokeMethod = delegateType.GetTypeInfo().GetDeclaredMethod("Invoke");
            if (invokeMethod == null)
            {
                throw new InvalidOperationException("The given delegate type does not have an Invoke method. Is this a compilation error?");
            }

            var constructorType = constructor.DeclaringType;
            var invokeReturnType = invokeMethod.ReturnType;

            bool isInvokeVoid = invokeReturnType == typeof(void);
            if (isInvokeVoid)
            {
                throw new InvalidOperationException("The return type the delegate is incompatible.");
            }

            var invokeParameterTypes = Ext.GetParameterTypes(invokeMethod);
            var constructorParameterTypes = Ext.GetParameterTypes(constructor);

            int count = invokeParameterTypes.Length;
            if (constructorParameterTypes.Length != count)
            {
                throw new InvalidOperationException("The number of parameters between the constructor and the delegate is not compatible.");
            }

            var parameterExpressions = new ParameterExpression[count];
            var arguments = new Expression[count];
            for (int i = 0; i < count; i++)
            {
                var argument = _GetArgumentExpression(i, constructorParameterTypes, invokeParameterTypes, parameterExpressions);
                arguments[i] = argument;
            }

            Expression resultExpression = Expression.New(constructor, arguments);

            if (constructorType != invokeReturnType)
            {
                resultExpression = Expression.Convert(resultExpression, invokeReturnType);
            }

            var lambda = Expression.Lambda(delegateType, resultExpression, parameterExpressions);
            var compiled = lambda.Compile();
            return compiled;
        }

        #endregion

#if !WINDOWS_PHONE

        #region GetConstructorDelegate

        private static YieldReaderWriterLockSlim _constructorsLock;
        private static readonly Dictionary<ConstructorInfo, FastDynamicDelegate> _constructors = new Dictionary<ConstructorInfo, FastDynamicDelegate>();
        private static readonly Func<ConstructorInfo, FastDynamicDelegate> _getConstructorDelegate = _CreateConstructorDelegate;

        /// <summary>
        ///     Gets a constructor invoker delegate for the given constructor info.
        ///     Using the delegate is much faster than calling the Invoke method on the constructor,
        ///     but if you invoke it only once, it will do no good as some time is spent compiling
        ///     such delegate.
        /// </summary>
        public static FastDynamicDelegate GetConstructorDelegate(ConstructorInfo constructor)
        {
            if (constructor == null)
            {
                throw new ArgumentNullException("constructor");
            }

            var result = Ext.GetOrCreateValue(_constructors, ref _constructorsLock, constructor, _getConstructorDelegate);
            return result;
        }

        private static FastDynamicDelegate _CreateConstructorDelegate(ConstructorInfo constructor)
        {
            var parametersExpression = Expression.Parameter(typeof(object[]), "parameters");

            var variables = new List<ParameterExpression>();
            var beforeInstructions = new List<Expression>();
            var afterInstructions = new List<Expression>();

            Expression[] arrayAccesses = null;
            var parameters = constructor.GetParameters();
            int count = parameters.Length;
            if (count != 0)
            {
                arrayAccesses = new Expression[count];
                for (int i = 0; i < count; i++)
                {
                    var parameter = parameters[i];
                    var parameterType = parameter.ParameterType;

                    var constantExpression = Expression.Constant(i);
                    Expression accessParameterExpression = Expression.ArrayAccess(parametersExpression, constantExpression);

                    if (parameterType.IsByRef)
                    {
                        parameterType = parameterType.GetElementType();

                        if (parameterType != typeof(object))
                        {
                            var variable = Expression.Variable(parameterType);
                            variables.Add(variable);
                            arrayAccesses[i] = variable;

                            if (!parameter.IsOut)
                            {
                                var effectiveAccessParameterExpression = accessParameterExpression;
                                if (parameterType != typeof(object))
                                {
                                    effectiveAccessParameterExpression = Expression.Convert(accessParameterExpression, parameterType);
                                }

                                var setIn = Expression.Assign(variable, effectiveAccessParameterExpression);
                                beforeInstructions.Add(setIn);
                            }

                            Expression accessVariable = variable;
                            if (parameterType != typeof(object))
                            {
                                accessVariable = Expression.Convert(variable, typeof(object));
                            }

                            var setOut = Expression.Assign(accessParameterExpression, accessVariable);
                            afterInstructions.Add(setOut);
                            continue;
                        }
                    }

                    if (parameterType != typeof(object))
                    {
                        accessParameterExpression = Expression.Convert(accessParameterExpression, parameterType);
                    }

                    arrayAccesses[i] = accessParameterExpression;
                }
            }

            var newExpression = Expression.New(constructor, arrayAccesses);
            var returnTarget = Expression.Label(typeof(object));
            var instructions = new List<Expression>();
            instructions.AddRange(beforeInstructions);

            ParameterExpression resultVariable = null;
            Expression body = newExpression;
            if (constructor.DeclaringType != typeof(object))
            {
                body = Expression.Convert(newExpression, typeof(object));
            }

            resultVariable = Expression.Variable(typeof(object));
            variables.Add(resultVariable);
            body = Expression.Assign(resultVariable, body);
            instructions.Add(body);

            instructions.AddRange(afterInstructions);
            var returnExpression = Expression.Return(returnTarget, resultVariable);
            instructions.Add(returnExpression);

            instructions.Add(Expression.Label(returnTarget, Expression.Constant(null, typeof(object))));
            body = Expression.Block(typeof(object), variables, instructions);

            var result = Expression.Lambda<FastDynamicDelegate>(body, parametersExpression);
            return result.Compile();
        }

        #endregion

        #region GetMethodCallDelegate

        private static YieldReaderWriterLockSlim _methodsLock;
        private static readonly Dictionary<MethodInfo, FastMethodCallDelegate> _methodsDictionary = new Dictionary<MethodInfo, FastMethodCallDelegate>();

        /// <summary>
        ///     Gets a delegate to call the given method in a fast manner.
        /// </summary>
        public static FastMethodCallDelegate GetMethodCallDelegate(MethodInfo method)
        {
            var result = Ext.GetOrCreateValue(_methodsDictionary, ref _methodsLock, method, _CreateMethodCallDelegate);
            return result;
        }

        private static FastMethodCallDelegate _CreateMethodCallDelegate(MethodInfo method)
        {
            var parametersExpression = Expression.Parameter(typeof(object[]));

            ParameterExpression targetExpression = Expression.Parameter(typeof(object));

            Expression castTarget = null;

            if (!method.IsStatic)
            {
                castTarget = targetExpression;
                if (method.ReturnType != typeof(object))
                {
                    castTarget = Expression.Convert(targetExpression, method.DeclaringType);
                }
            }

            var variables = new List<ParameterExpression>();
            var beforeInstructions = new List<Expression>();
            var afterInstructions = new List<Expression>();

            Expression[] arrayAccesses = null;
            var parameters = method.GetParameters();
            int count = parameters.Length;
            if (count != 0)
            {
                arrayAccesses = new Expression[count];
                for (int i = 0; i < count; i++)
                {
                    var parameter = parameters[i];
                    var parameterType = parameter.ParameterType;

                    var constantExpression = Expression.Constant(i);
                    Expression accessParameterExpression = Expression.ArrayAccess(parametersExpression, constantExpression);

                    if (parameterType.IsByRef)
                    {
                        parameterType = parameterType.GetElementType();

                        if (parameterType != typeof(object))
                        {
                            var variable = Expression.Variable(parameterType);
                            variables.Add(variable);
                            arrayAccesses[i] = variable;

                            if (!parameter.IsOut)
                            {
                                var effectiveAccessParameterExpression = accessParameterExpression;
                                if (parameterType != typeof(object))
                                {
                                    effectiveAccessParameterExpression = Expression.Convert(accessParameterExpression, parameterType);
                                }

                                var setIn = Expression.Assign(variable, effectiveAccessParameterExpression);
                                beforeInstructions.Add(setIn);
                            }

                            Expression accessVariable = variable;
                            if (parameterType != typeof(object))
                            {
                                accessVariable = Expression.Convert(variable, typeof(object));
                            }

                            var setOut = Expression.Assign(accessParameterExpression, accessVariable);
                            afterInstructions.Add(setOut);
                            continue;
                        }
                    }

                    if (parameterType != typeof(object))
                    {
                        accessParameterExpression = Expression.Convert(accessParameterExpression, parameterType);
                    }

                    arrayAccesses[i] = accessParameterExpression;
                }
            }

            MethodCallExpression callExpression;

            if (method.IsStatic)
            {
                callExpression = Expression.Call(method, arrayAccesses);
            }
            else
            {
                callExpression = Expression.Call(castTarget, method, arrayAccesses);
            }

            var returnTarget = Expression.Label(typeof(object));
            var instructions = new List<Expression>();
            instructions.AddRange(beforeInstructions);

            ParameterExpression resultVariable = null;
            Expression body = callExpression;
            if (method.ReturnType != typeof(void))
            {
                if (method.ReturnType != typeof(object))
                {
                    body = Expression.Convert(callExpression, typeof(object));
                }

                resultVariable = Expression.Variable(typeof(object));
                variables.Add(resultVariable);
                body = Expression.Assign(resultVariable, body);
            }
            instructions.Add(body);

            instructions.AddRange(afterInstructions);

            if (method.ReturnType == typeof(void))
            {
                var returnExpression = Expression.Return(returnTarget, Expression.Constant(null, typeof(object)), typeof(object));
                instructions.Add(returnExpression);
            }
            else
            {
                var returnExpression = Expression.Return(returnTarget, resultVariable);
                instructions.Add(returnExpression);
            }

            instructions.Add(Expression.Label(returnTarget, Expression.Constant(null, typeof(object))));
            body = Expression.Block(typeof(object), variables, instructions);

            var result = Expression.Lambda<FastMethodCallDelegate>(body, targetExpression, parametersExpression);

            return result.Compile();
        }

        #endregion

#endif

        #region GetMethodCallDelegate<T>

        /// <summary>
        ///     Creates a method call delegate for the given method info.
        ///     The delegateType (T) should have the same amount of parameters as the method. Note
        ///     that non-static methods have a first parameter to represent the instance.
        /// </summary>
        public static T GetMethodCallDelegate<T>(MethodInfo method)
        {
            object result = GetMethodCallDelegate(method, typeof(T));
            return (T)result;
        }

        private static YieldReaderWriterLockSlim _getTypedMethodCallLock;
        private static readonly Dictionary<KeyValuePair<MethodInfo, Type>, Delegate> _getTypedMethodCallDictionary = new Dictionary<KeyValuePair<MethodInfo, Type>, Delegate>();
        private static readonly Func<KeyValuePair<MethodInfo, Type>, Delegate> _getTypedMethodCallDelegate = _GetTypedMethodCallDelegate;

        /// <summary>
        ///     Creates a method call delegate for the given method info.
        ///     The delegateType should have the same amount of parameters as the method. Note
        ///     that non-static methods have a first parameter to represent the instance.
        /// </summary>
        public static Delegate GetMethodCallDelegate(MethodInfo method, Type delegateType)
        {
            if (method == null)
            {
                throw new ArgumentNullException("method");
            }

            if (!delegateType.GetTypeInfo().IsSubclassOf(typeof(Delegate)))
            {
                throw new ArgumentException("delegateType is not a Delegate.", "delegateType");
            }

            var pair = new KeyValuePair<MethodInfo, Type>(method, delegateType);
            var result = Ext.GetOrCreateValue(_getTypedMethodCallDictionary, ref _getTypedMethodCallLock, pair, _getTypedMethodCallDelegate);
            return result;
        }

        private static Delegate _GetTypedMethodCallDelegate(KeyValuePair<MethodInfo, Type> pair)
        {
            var method = pair.Key;
            var delegateType = pair.Value;

            var invokeMethod = delegateType.GetTypeInfo().GetDeclaredMethod("Invoke");
            if (invokeMethod == null)
            {
                throw new InvalidOperationException("The given delegate type does not have an Invoke method. Is this a compilation error?");
            }

            var methodReturnType = method.ReturnType;
            var invokeReturnType = invokeMethod.ReturnType;

            bool isMethodVoid = methodReturnType == typeof(void);
            bool isInvokeVoid = invokeReturnType == typeof(void);
            if (isMethodVoid != isInvokeVoid)
            {
                throw new InvalidOperationException("The return type of the method and the delegate is incompatible.");
            }

            var invokeParameterTypes = Ext.GetParameterTypes(invokeMethod);
            var methodParameterTypes = new List<Type>();
            if (!method.IsStatic)
            {
                methodParameterTypes.Add(method.DeclaringType);
            }

            methodParameterTypes.AddRange(Ext.GetParameterTypes(method));

            int count = invokeParameterTypes.Length;
            if (methodParameterTypes.Count != count)
            {
                throw new InvalidOperationException(
                    "The number of parameters between the method and the delegate is not compatible. Note that non-static methods have the additional \"this\" parameter as the first one.");
            }

            var parameterExpressions = new ParameterExpression[count];

            int startIndex = 0;
            int argumentCount = count;
            if (!method.IsStatic)
            {
                startIndex = 1;
                argumentCount--;
            }

            var arguments = new Expression[argumentCount];
            for (int i = 0; i < argumentCount; i++)
            {
                var argument = _GetArgumentExpression(i + startIndex, methodParameterTypes, invokeParameterTypes, parameterExpressions);
                arguments[i] = argument;
            }

            MethodCallExpression callExpression;
            if (method.IsStatic)
            {
                callExpression = Expression.Call(method, arguments);
            }
            else
            {
                var instanceExpression = _GetArgumentExpression(0, methodParameterTypes, invokeParameterTypes, parameterExpressions);
                callExpression = Expression.Call(instanceExpression, method, arguments);
            }

            Expression resultExpression = callExpression;
            if (methodReturnType != invokeReturnType)
            {
                resultExpression = Expression.Convert(resultExpression, invokeReturnType);
            }

            var lambda = Expression.Lambda(delegateType, resultExpression, parameterExpressions);
            var compiled = lambda.Compile();
            return compiled;
        }

        private static Expression _GetArgumentExpression(int index, IList<Type> methodParameterTypes, Type[] invokeParameterTypes, ParameterExpression[] parameterExpressions)
        {
            var invokeParameterType = invokeParameterTypes[index];
            var methodParameterType = methodParameterTypes[index];

            var parameter = Expression.Parameter(invokeParameterType, "P" + index);
            parameterExpressions[index] = parameter;
            if (methodParameterType == invokeParameterType)
            {
                return parameter;
            }

            var convert = Expression.Convert(parameter, methodParameterType);
            return convert;
        }

        #endregion

        #region GetPropertyGetterDelegate

        private static YieldReaderWriterLockSlim _getPropertyLock;
        private static readonly Dictionary<PropertyInfo, Func<object, object>> _getPropertiesDictionary = new Dictionary<PropertyInfo, Func<object, object>>();
        private static readonly Func<PropertyInfo, Func<object, object>> _getPropertyGetterDelegate = GetPropertyGetterDelegate<object, object>;

        /// <summary>
        ///     Gets a delegate to read values from the given property in a very fast manner.
        /// </summary>
        public static Func<object, object> GetPropertyGetterDelegate(PropertyInfo property)
        {
            var result = Ext.GetOrCreateValue(_getPropertiesDictionary, ref _getPropertyLock, property, _getPropertyGetterDelegate);
            return result;
        }

        private static YieldReaderWriterLockSlim _getTypedPropertyLock;
        private static readonly Dictionary<KeyValuePair<Type, PropertyInfo>, Delegate> _getTypedPropertiesDictionary = new Dictionary<KeyValuePair<Type, PropertyInfo>, Delegate>();
        private static readonly Func<KeyValuePair<Type, PropertyInfo>, Delegate> _getTypedPropertyGetterDelegate = _GetPropertyGetterDelegate;

        /// <summary>
        ///     Gets a delegate to read values from the given property in a very fast manner.
        ///     The result will be already cast or will even avoid casts if the
        ///     generic types are correct.
        /// </summary>
        public static Func<TInstance, TOutput> GetPropertyGetterDelegate<TInstance, TOutput>(PropertyInfo property)
        {
            if (property == null)
            {
                throw new ArgumentNullException("property");
            }

            var pair = new KeyValuePair<Type, PropertyInfo>(typeof(Func<TInstance, TOutput>), property);
            var result = Ext.GetOrCreateValue(_getTypedPropertiesDictionary, ref _getTypedPropertyLock, pair, _getTypedPropertyGetterDelegate);
            return (Func<TInstance, TOutput>)result;
        }

        private static Delegate _GetPropertyGetterDelegate(KeyValuePair<Type, PropertyInfo> pair)
        {
            var funcType = pair.Key;
            var property = pair.Value;

            var funcArguments = funcType.GetTypeInfo().GenericTypeArguments;
            var instanceType = funcArguments[0];
            var resultType = funcArguments[1];

            var parameter = Expression.Parameter(instanceType, "instance");
            Expression resultExpression;

            var getMethod = property.GetMethod;
            if (getMethod == null)
            {
                throw new ArgumentException("Property " + property.Name + " can't be read.", "read");
            }

            if (getMethod.IsStatic)
            {
                resultExpression = Expression.MakeMemberAccess(null, property);
            }
            else
            {
                Expression readParameter = parameter;

                if (property.DeclaringType != instanceType)
                {
                    readParameter = Expression.Convert(parameter, property.DeclaringType);
                }

                resultExpression = Expression.MakeMemberAccess(readParameter, property);
            }

            if (property.PropertyType != resultType)
            {
                resultExpression = Expression.Convert(resultExpression, resultType);
            }

            var lambda = Expression.Lambda(funcType, resultExpression, parameter);

            var result = lambda.Compile();
            return result;
        }

        #endregion

        #region GetPropertySetterDelegate

        private static YieldReaderWriterLockSlim _setPropertyLock;
        private static readonly Dictionary<PropertyInfo, Action<object, object>> _setPropertiesDictionary = new Dictionary<PropertyInfo, Action<object, object>>();
        private static readonly Func<PropertyInfo, Action<object, object>> _getPropertySetterDelegate = GetPropertySetterDelegate<object, object>;

        /// <summary>
        ///     Gets a delegate that can be used to do very fast sets on the given property.
        /// </summary>
        public static Action<object, object> GetPropertySetterDelegate(PropertyInfo property)
        {
            var result = Ext.GetOrCreateValue(_setPropertiesDictionary, ref _setPropertyLock, property, _getPropertySetterDelegate);
            return result;
        }

        private static YieldReaderWriterLockSlim _typedPropertySetterDelegatesLock;
        private static readonly Dictionary<KeyValuePair<Type, PropertyInfo>, Delegate> _typedPropertySetterDelegatesDictionary = new Dictionary<KeyValuePair<Type, PropertyInfo>, Delegate>();
        private static readonly Func<KeyValuePair<Type, PropertyInfo>, Delegate> _typedGetPropertySetterDelegate = _TypedGetPropertySetterDelegate;

        /// <summary>
        ///     Gets a delegate that can be used to do very fast sets on the given property.
        ///     If the generic types are correct, casts can be avoided to improve performance
        ///     even further.
        /// </summary>
        public static Action<TInstance, TValue> GetPropertySetterDelegate<TInstance, TValue>(PropertyInfo property)
        {
            if (property == null)
            {
                throw new ArgumentNullException("property");
            }

            var pair = new KeyValuePair<Type, PropertyInfo>(typeof(Action<TInstance, TValue>), property);
            var result = Ext.GetOrCreateValue(_typedPropertySetterDelegatesDictionary, ref _typedPropertySetterDelegatesLock, pair, _typedGetPropertySetterDelegate);
            return (Action<TInstance, TValue>)result;
        }

        private static Delegate _TypedGetPropertySetterDelegate(KeyValuePair<Type, PropertyInfo> pair)
        {
            var actionType = pair.Key;
            var property = pair.Value;
            var actionArguments = actionType.GenericTypeArguments;
            var instanceType = actionArguments[0];
            var valueType = actionArguments[1];

            var instanceParameter = Expression.Parameter(instanceType, "instance");

            var valueParameter = Expression.Parameter(valueType, "value");
            Expression readValueParameter = valueParameter;
            if (property.PropertyType != valueType)
            {
                readValueParameter = Expression.Convert(valueParameter, property.PropertyType);
            }

            // .Net 3.5 does not have assign
            // but we can call the set method directly (and we need it to test for static).
            var setMethod = property.SetMethod;
            if (setMethod == null)
            {
                throw new ArgumentException("Property " + property.Name + " is read-only.", "property");
            }

            Expression setExpression;
            if (setMethod.IsStatic)
            {
                setExpression = Expression.Call(setMethod, readValueParameter);
            }
            else
            {
                Expression readInstanceParameter = instanceParameter;
                if (property.DeclaringType != instanceType)
                {
                    readInstanceParameter = Expression.Convert(instanceParameter, property.DeclaringType);
                }

                setExpression = Expression.Call(readInstanceParameter, setMethod, readValueParameter);
            }

            var lambda = Expression.Lambda(actionType, setExpression, instanceParameter, valueParameter);
            var result = lambda.Compile();
            return result;
        }

        #endregion

        #region GetEventAdder

        private static YieldReaderWriterLockSlim _addEventLock;
        private static readonly Dictionary<EventInfo, Action<object, Delegate>> _addEventDictionary = new Dictionary<EventInfo, Action<object, Delegate>>();
        private static readonly Func<EventInfo, Action<object, Delegate>> _getEventAdderDelegate = GetEventAdderDelegate<object, Delegate>;

        /// <summary>
        ///     Gets a delegate to do fast "event add"
        /// </summary>
        public static Action<object, Delegate> GetEventAdderDelegate(EventInfo eventInfo)
        {
            var result = Ext.GetOrCreateValue(_addEventDictionary, ref _addEventLock, eventInfo, _getEventAdderDelegate);
            return result;
        }

        /// <summary>
        ///     Gets a delegate to do fast "event add".
        ///     Can avoid casts if the right generic types are given.
        /// </summary>
        public static Action<TInstance, TDelegate> GetEventAdderDelegate<TInstance, TDelegate>(EventInfo eventInfo)
        {
            return _GetEventDelegate<TInstance, TDelegate>(eventInfo.AddMethod, eventInfo.EventHandlerType);
        }

        #endregion

        #region GetEventRemoverDelegate

        private static YieldReaderWriterLockSlim _removeEventLock;
        private static readonly Dictionary<EventInfo, Action<object, Delegate>> _removeEventDictionary = new Dictionary<EventInfo, Action<object, Delegate>>();
        private static readonly Func<EventInfo, Action<object, Delegate>> _getEventRemoverDelegate = GetEventRemoverDelegate<object, Delegate>;

        /// <summary>
        ///     Gets a delegate to do fast "event remove" calls.
        /// </summary>
        public static Action<object, Delegate> GetEventRemoverDelegate(EventInfo eventInfo)
        {
            var result = Ext.GetOrCreateValue(_removeEventDictionary, ref _removeEventLock, eventInfo, _getEventRemoverDelegate);
            return result;
        }

        /// <summary>
        ///     Gets a delegate to do fast "event remove" calls.
        ///     Can avoid casts if the right generic types are given.
        /// </summary>
        public static Action<TInstance, TDelegate> GetEventRemoverDelegate<TInstance, TDelegate>(EventInfo eventInfo)
        {
            return _GetEventDelegate<TInstance, TDelegate>(eventInfo.RemoveMethod, eventInfo.EventHandlerType);
        }

        #endregion

        #region _GetEventDelegate

        private static Action<TInstance, TDelegate> _GetEventDelegate<TInstance, TDelegate>(MethodInfo method, Type handlerType)
        {
            var instanceParameter = Expression.Parameter(typeof(TInstance), "instance");
            var handlerParameter = Expression.Parameter(typeof(TDelegate), "handler");
            Expression readHandlerParameter = handlerParameter;
            if (handlerType != typeof(TDelegate))
            {
                readHandlerParameter = Expression.Convert(handlerParameter, handlerType);
            }

            Expression callExpression;
            if (method.IsStatic)
            {
                callExpression = Expression.Call(method, readHandlerParameter);
            }
            else
            {
                Expression readInstanceParameter = instanceParameter;
                if (method.DeclaringType != typeof(TInstance))
                {
                    readInstanceParameter = Expression.Convert(instanceParameter, method.DeclaringType);
                }

                callExpression = Expression.Call(readInstanceParameter, method, readHandlerParameter);
            }

            var lambda = Expression.Lambda<Action<TInstance, TDelegate>>(callExpression, instanceParameter, handlerParameter);
            var result = lambda.Compile();
            return result;
        }

        #endregion

        #region GetFieldGetterDelegate

        private static YieldReaderWriterLockSlim _getFieldLock;
        private static readonly Dictionary<FieldInfo, Func<object, object>> _getFieldsDictionary = new Dictionary<FieldInfo, Func<object, object>>();
        private static readonly Func<FieldInfo, Func<object, object>> _getFieldGetterDelegate = GetFieldGetterDelegate<object, object>;

        /// <summary>
        ///     Gets a delegate to read values from the given field in a very fast manner.
        /// </summary>
        public static Func<object, object> GetFieldGetterDelegate(FieldInfo field)
        {
            var result = Ext.GetOrCreateValue(_getFieldsDictionary, ref _getFieldLock, field, _getFieldGetterDelegate);
            return result;
        }

        private static YieldReaderWriterLockSlim _getTypedFieldLock;
        private static readonly Dictionary<KeyValuePair<Type, FieldInfo>, Delegate> _getTypedFieldsDictionary = new Dictionary<KeyValuePair<Type, FieldInfo>, Delegate>();
        private static readonly Func<KeyValuePair<Type, FieldInfo>, Delegate> _getTypedFieldGetterDelegate = _GetFieldGetterDelegate;

        /// <summary>
        ///     Gets a delegate to read values from the given field in a very fast manner.
        ///     The result will be already cast or will even avoid casts if the
        ///     generic types are correct.
        /// </summary>
        public static Func<TInstance, TOutput> GetFieldGetterDelegate<TInstance, TOutput>(FieldInfo field)
        {
            if (field == null)
            {
                throw new ArgumentNullException("field");
            }

            var pair = new KeyValuePair<Type, FieldInfo>(typeof(Func<TInstance, TOutput>), field);
            var result = Ext.GetOrCreateValue(_getTypedFieldsDictionary, ref _getTypedFieldLock, pair, _getTypedFieldGetterDelegate);
            return (Func<TInstance, TOutput>)result;
        }

        private static Delegate _GetFieldGetterDelegate(KeyValuePair<Type, FieldInfo> pair)
        {
            var funcType = pair.Key;
            var field = pair.Value;

            var funcArguments = funcType.GetTypeInfo().GenericTypeArguments;
            var instanceType = funcArguments[0];
            var resultType = funcArguments[1];

            var parameter = Expression.Parameter(instanceType, "instance");
            Expression resultExpression;

            if (field.IsStatic)
            {
                resultExpression = Expression.MakeMemberAccess(null, field);
            }
            else
            {
                Expression readParameter = parameter;

                if (field.DeclaringType != instanceType)
                {
                    readParameter = Expression.Convert(parameter, field.DeclaringType);
                }

                resultExpression = Expression.MakeMemberAccess(readParameter, field);
            }

            if (field.FieldType != resultType)
            {
                resultExpression = Expression.Convert(resultExpression, resultType);
            }

            var lambda = Expression.Lambda(funcType, resultExpression, parameter);

            var result = lambda.Compile();
            return result;
        }

        #endregion

#if !WINDOWS_PHONE

        #region GetFastDynamicDelegate

        /// <summary>
        ///     You have an untyped delegate? Then get another delegate to invoke it faster.
        /// </summary>
        public static FastDynamicDelegate GetFastDynamicDelegate(Delegate realDelegate)
        {
            var result = GetMethodCallDelegate(realDelegate.GetMethodInfo());
            return (parameters) => result(realDelegate.Target, parameters);
        }

        #endregion

#endif

        #region YieldReaderWriterLockSlim

        /// <summary>
        ///     A "real slim" reader writer lock.
        ///     Many readers can read at a time and only one writer is allowed.
        ///     Reads can be recursive, but a try to a recursive write will cause a dead-lock.
        ///     Note that this is a struct, so don't assign it to a local variable.
        /// </summary>
        internal struct YieldReaderWriterLockSlim
        {
            #region Consts

            private const int _writeBitShift = 24;
            private const int _upgradeBitShift = 16;

            private const LockIntegralType _writeLockValue = ((LockIntegralType)1) << _writeBitShift;
            private const LockIntegralType _writeUnlockValue = -_writeLockValue;
            private const LockIntegralType _upgradeLockValue = ((LockIntegralType)1) << _upgradeBitShift;
            private const LockIntegralType _upgradeUnlockValue = -_upgradeLockValue;
            private const LockIntegralType _allReadsValue = _upgradeLockValue - 1;
            private const LockIntegralType _someExclusiveLockValue = _writeLockValue | _upgradeLockValue;
            private const LockIntegralType _someExclusiveUnlockValue = -_someExclusiveLockValue;

            #endregion

            #region Fields

            private LockIntegralType _lockValue;

            #endregion

            #region EnterReadLock

            /// <summary>
            ///     Enters a read lock.
            /// </summary>
            public void EnterReadLock()
            {
                while (true)
                {
                    LockIntegralType result = Interlocked.Increment(ref this._lockValue);
                    if ((result >> _writeBitShift) == 0)
                    {
                        return;
                    }

                    Interlocked.Decrement(ref this._lockValue);

                    while (true)
                    {
#if SILVERLIGHT
									Thread.Sleep(1);
#else
                        //Thread.Yield();
                        Task.Delay(0);
#endif

                        result = Interlocked.CompareExchange(ref this._lockValue, 1, 0);
                        if (result == 0)
                        {
                            return;
                        }

                        if ((result >> _writeBitShift) == 0)
                        {
                            break;
                        }
                    }
                }
            }

            #endregion

            #region ExitReadLock

            /// <summary>
            ///     Exits a read-lock. Take care not to exit more times than you entered, as there is no check for that.
            /// </summary>
            public void ExitReadLock()
            {
                Interlocked.Decrement(ref this._lockValue);
            }

            #endregion

            #region EnterUpgradeableLock

            /// <summary>
            ///     Enters an upgradeable lock (it is a read lock, but it can be upgraded).
            ///     Only one upgradeable lock is allowed at a time.
            /// </summary>
            public void EnterUpgradeableLock()
            {
                while (true)
                {
                    LockIntegralType result = Interlocked.Add(ref this._lockValue, _upgradeLockValue);
                    if ((result >> _upgradeBitShift) == 1)
                    {
                        return;
                    }

                    Interlocked.Add(ref this._lockValue, _upgradeUnlockValue);

                    while (true)
                    {
#if SILVERLIGHT
									Thread.Sleep(1);
#else
                        //Thread.Yield();
                        Task.Delay(0);
#endif

                        result = Interlocked.CompareExchange(ref this._lockValue, _upgradeLockValue, 0);
                        if (result == 0)
                        {
                            return;
                        }

                        if ((result >> _upgradeBitShift) == 0)
                        {
                            break;
                        }
                    }
                }
            }

            #endregion

            #region ExitUpgradeableLock

            /// <summary>
            ///     Exits a previously obtained upgradeable lock.
            /// </summary>
            public void ExitUpgradeableLock()
            {
                Interlocked.Add(ref this._lockValue, _upgradeUnlockValue);
            }

            #endregion

            #region UpgradeToWriteLock

            /// <summary>
            ///     upgrades to write-lock. You must already own a Upgradeable lock and you must first exit the write lock then the
            ///     Upgradeable lock.
            /// </summary>
            public void UpgradeToWriteLock()
            {
                LockIntegralType result = Interlocked.Add(ref this._lockValue, _writeLockValue);

                while ((result & _allReadsValue) != 0)
                {
#if SILVERLIGHT
								Thread.Sleep(1);
#else
                    //Thread.Yield();
                    Task.Delay(0);
#endif

                    result = Interlocked.CompareExchange(ref this._lockValue, 0, 0);
                    //result = Interlocked.Read(ref _lockValue);
                }
            }

            #endregion

            #region ExitUpgradedLock

            /// <summary>
            ///     Releases the Upgradeable lock and the upgraded version of it (the write lock)
            ///     at the same time.
            ///     Releasing the write lock and the upgradeable lock has the same effect, but
            ///     it's slower.
            /// </summary>
            public void ExitUpgradedLock()
            {
                Interlocked.Add(ref this._lockValue, _someExclusiveUnlockValue);
            }

            #endregion

            #region EnterWriteLock

            /// <summary>
            ///     Enters write-lock.
            /// </summary>
            public void EnterWriteLock()
            {
                LockIntegralType result = Interlocked.Add(ref this._lockValue, _writeLockValue);
                if (result == _writeLockValue)
                {
                    return;
                }

                // we need to try again.
                Interlocked.Add(ref this._lockValue, _writeUnlockValue);
                for (int i = 0; i < 100; i++)
                {
#if SILVERLIGHT
								Thread.Sleep(1);
#else
                    //Thread.Yield();
                    Task.Delay(0);
#endif

                    result = Interlocked.CompareExchange(ref this._lockValue, _writeLockValue, 0);
                    if (result == 0)
                    {
                        return;
                    }

                    // try to be the first locker.
                    if ((result >> _writeBitShift) == 0)
                    {
                        break;
                    }
                }

                // From this moment, we have priority.
                while (true)
                {
                    result = Interlocked.Add(ref this._lockValue, _writeLockValue);
                    if (result == _writeLockValue)
                    {
                        return;
                    }

                    if ((result >> _writeBitShift) == 1)
                    {
                        // we obtained the write lock, but there may be readers,
                        // so we wait until they release the lock.
                        while (true)
                        {
#if SILVERLIGHT
										Thread.Sleep(1);
#else
                            //Thread.Yield();
                            Task.Delay(0);
#endif

                            result = Interlocked.CompareExchange(ref this._lockValue, 0, 0);
                            //result = Interlocked.Read(ref _lockValue);
                            if (result == _writeLockValue)
                            {
                                return;
                            }
                        }
                    }
                    else
                    {
                        // we need to try again.
                        Interlocked.Add(ref this._lockValue, _writeUnlockValue);
                        while (true)
                        {
#if SILVERLIGHT
										Thread.Sleep(1);
#else
                            //Thread.Yield();
                            Task.Delay(0);
#endif

                            result = Interlocked.CompareExchange(ref this._lockValue, _writeLockValue, 0);
                            if (result == 0)
                            {
                                return;
                            }

                            // try to be the first locker.
                            if ((result >> _writeBitShift) == 0)
                            {
                                break;
                            }
                        }
                    }
                }
            }

            #endregion

            #region ExitWriteLock

            /// <summary>
            ///     Exits write lock. Take care to exit only when you entered, as there is no check for that.
            /// </summary>
            public void ExitWriteLock()
            {
                Interlocked.Add(ref this._lockValue, _writeUnlockValue);
            }

            #endregion
        }

        #endregion

        #region Ext

        internal static class Ext
        {
            public static TValue GetOrCreateValue<TKey, TValue>(Dictionary<TKey, TValue> dictionary, ref YieldReaderWriterLockSlim readerWriterLock, TKey key, Func<TKey, TValue> createValue)
            {
                TValue result;
                readerWriterLock.EnterReadLock();
                try
                {
                    if (dictionary.TryGetValue(key, out result))
                    {
                        return result;
                    }
                }
                finally
                {
                    readerWriterLock.ExitReadLock();
                }

                bool upgraded = false;
                readerWriterLock.EnterUpgradeableLock();
                try
                {
                    if (dictionary.TryGetValue(key, out result))
                    {
                        return result;
                    }

                    result = createValue(key);
                    readerWriterLock.UpgradeToWriteLock();
                    upgraded = true;
                    dictionary.Add(key, result);
                }
                finally
                {
                    if (upgraded)
                    {
                        readerWriterLock.ExitUpgradedLock();
                    }
                    else
                    {
                        readerWriterLock.ExitUpgradeableLock();
                    }
                }

                return result;
            }

            private static readonly Dictionary<MethodBase, Type[]> _parameterTypes = new Dictionary<MethodBase, Type[]>();

            public static Type[] GetParameterTypes(MethodBase methodInfo)
            {
                if (methodInfo == null)
                {
                    throw new ArgumentNullException("methodInfo");
                }

                Type[] result;
                lock (_parameterTypes)
                {
                    if (!_parameterTypes.TryGetValue(methodInfo, out result))
                    {
                        var parameters = methodInfo.GetParameters();

                        result = new Type[parameters.Length];
                        for (int i = 0; i < parameters.Length; i++)
                        {
                            result[i] = parameters[i].ParameterType;
                        }

                        _parameterTypes.Add(methodInfo, result);
                    }
                }

                if (result.Length == 0)
                {
                    return result;
                }

                return (Type[])result.Clone();
            }
        }

        #endregion
    }

    /// <summary>
    ///     This is a typed version of reflection helper, so your expression already starts with a know
    ///     object type (used when you don't have an already instantiated object).
    /// </summary>
    public static class ReflectionHelper<ForType>
    {
        #region GetMember

        /// <summary>
        ///     Gets a member by it's expression usage.
        ///     For example, GetMember((obj) => obj.GetType()) will return the
        ///     GetType method.
        /// </summary>
        public static MemberInfo GetMember<T>(Expression<Func<ForType, T>> expression)
        {
            if (expression == null)
            {
                throw new ArgumentNullException("expression");
            }

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

        #endregion

        #region GetField

        /// <summary>
        ///     Gets a field from a sample usage.
        ///     Example: GetField((obj) => obj.SomeField) will return the FieldInfo of
        ///     EmptyTypes.
        /// </summary>
        public static FieldInfo GetField<T>(Expression<Func<ForType, T>> expression)
        {
            return (FieldInfo)GetMember(expression);
        }

        #endregion

        #region GetProperty

        /// <summary>
        ///     Gets a property from a sample usage.
        ///     Example: GetProperty((str) => str.Length) will return the property info
        ///     of Length.
        /// </summary>
        public static PropertyInfo GetProperty<T>(Expression<Func<ForType, T>> expression)
        {
            return (PropertyInfo)GetMember(expression);
        }

        #endregion

        #region GetMethod

        /// <summary>
        ///     Gets a method info of a void method.
        ///     Example: GetMethod((obj) => obj.SomeCall("")); will return the
        ///     MethodInfo of SomeCall that receives a single argument.
        /// </summary>
        public static MethodInfo GetMethod(Expression<Action<ForType>> expression)
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
        ///     Example: GetMethod((obj) => obj.SomeCall()); will return the method info
        ///     of SomeCall.
        /// </summary>
        public static MethodInfo GetMethod<T>(Expression<Func<ForType, T>> expression)
        {
            return (MethodInfo)GetMember(expression);
        }

        #endregion
    }
}