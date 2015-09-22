using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Microsoft.CSharp;

using Xunit;

namespace TypeConverter.Tests
{
    public class TypeHelperTests
    {
        [Fact]
        public void ImplicitlyCastable()
        {
            this.RunTests((from, to) => from.IsImplicitlyCastableTo(to), isImplicitCastable: true);
        }

        [Fact]
        public void ExplicitlyCastable()
        {
            this.RunTests((from, to) => from.IsCastableTo(to), isImplicitCastable: false);
        }

        /// <summary>
        ///     Validates the given implementation function for either implicit or explicit conversion
        /// </summary>
        private void RunTests(Func<Type, Type, bool> isCastableFunc, bool isImplicitCastable)
        {
            // gather types
            var primitives = typeof(object).GetTypeInfo().Assembly.DefinedTypes.Where(t => t.IsPrimitive).ToArray();
            var simpleTypes = new[] { typeof(string), typeof(DateTime), typeof(decimal), typeof(object), typeof(DateTimeOffset), typeof(TimeSpan), typeof(StringSplitOptions), typeof(DateTimeKind) };
            var variantTypes = new[]
                                   {
                                       typeof(string[]),
                                       typeof(object[]),
                                       typeof(IEnumerable<string>),
                                       typeof(IEnumerable<object>),
                                       typeof(Func<string>),
                                       typeof(Func<object>),
                                       typeof(Action<string>),
                                       typeof(Action<object>)
                                   };
            var conversionOperators = new[] { typeof(Operators), typeof(Operators2), typeof(DerivedOperators), typeof(OperatorsStruct) };
            var typesToConsider = primitives.Concat(simpleTypes).Concat(variantTypes).Concat(conversionOperators).ToArray();
            var allTypesToConsider = typesToConsider.Concat(typesToConsider.Where(t => t.IsValueType).Select(t => typeof(Nullable<>).MakeGenericType(t)));

            // generate test cases
            var testCases = this.GenerateTestCases(allTypesToConsider, isImplicitCastable);

            // collect errors
            var mistakes = new List<string>();
            foreach (var testCase in testCases)
            {
                var isCastable = isCastableFunc(testCase.Item1, testCase.Item2);
                if (!isCastable && (testCase.Item3 == null))
                {
                    mistakes.Add(
                        string.Format("{0} => {1}: got {2} for {3} cast", testCase.Item1, testCase.Item2, isCastable ? "was successful" : "failed", isImplicitCastable ? "implicit" : "explicit"));
                }
            }
            Assert.True(mistakes.Count == 0, string.Join(Environment.NewLine, new[] { mistakes.Count + " errors" }.Concat(mistakes)));
        }

        private List<Tuple<Type, Type, CompilerError>> GenerateTestCases(IEnumerable<Type> types, bool isImplicit)
        {
            // gather all pairs
            var typeCrossProduct = types.SelectMany(t => types, (from, to) => new { from, to }).Select((t, index) => new { t.from, t.to, index }).ToArray();

            // create the code to pass to the compiler
            var code = string.Join(
                Environment.NewLine,
                new[] { "namespace A { public class B { static T Get<T>() { return default(T); } public void C() {" }.Concat(
                    typeCrossProduct.Select(t => string.Format("{0} var{1} = {2}Get<{3}>();", GetName(t.to), t.index, isImplicit ? string.Empty : "(" + GetName(t.to) + ")", GetName(t.from))))
                    .Concat(new[] { "}}}" }));

            // compile the code
            var provider = new CSharpCodeProvider();
            var compilerParams = new CompilerParameters();
            compilerParams.ReferencedAssemblies.Add(this.GetType().Assembly.Location); // reference the current assembly!
            compilerParams.GenerateExecutable = false;
            compilerParams.GenerateInMemory = true;
            var compilationResult = provider.CompileAssemblyFromSource(compilerParams, code);

            // determine the outcome of each conversion by matching compiler errors with conversions by line #
            var cases = typeCrossProduct.GroupJoin(compilationResult.Errors.Cast<CompilerError>(), t => t.index, e => e.Line - 2, (t, e) => Tuple.Create(t.from, t.to, e.FirstOrDefault())).ToList();

            // add a special case
            // this can't be verified by the normal means, since it's a private class
            cases.Add(Tuple.Create(typeof(PrivateOperators), typeof(int), default(CompilerError)));

            return cases;
        }

        /// <summary>
        ///     Gets a C# name for the given type
        /// </summary>
        private static string GetName(Type type)
        {
            if (!type.IsGenericType)
            {
                return type.ToString();
            }

            return string.Format("{0}.{1}<{2}>", type.Namespace, type.Name.Substring(0, type.Name.IndexOf('`')), string.Join(", ", type.GetGenericArguments().Select(GetName)));
        }

        private class PrivateOperators
        {
            public static implicit operator int(PrivateOperators o)
            {
                return 1;
            }
        }
    }
}