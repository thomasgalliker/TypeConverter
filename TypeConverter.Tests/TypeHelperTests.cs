using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

using FluentAssertions;

using Microsoft.CSharp;

using TypeConverter.Extensions;
using TypeConverter.Tests.Stubs;
using TypeConverter.Utils;

using Xunit;
using Xunit.Abstractions;

namespace TypeConverter.Tests
{
    public class TypeHelperTests
    {
        private readonly ITestOutputHelper output;

        public TypeHelperTests(ITestOutputHelper output)
        {
            this.output = output;
        }

        ////[Fact]
        ////public void ShouldSucceedIsImplicitlyCastableTo()
        ////{
        ////    this.RunTests((from, to) => from.IsImplicitlyCastableTo(to), isImplicitCastable: true);
        ////}

        [Fact]
        public void ShouldSucceedCastToImplicitly()
        {
            TypeHelper.IsCacheEnabled = false;
            bool isImplicit = true;

            this.RunTests(
                (testCase) =>
                {
                    // Arrange
                    var value = this.GenerateValueForType(testCase.SourceType);
                    var generatedTestSuccessful = this.CastValueWithGeneratedCode(value, testCase.SourceType, testCase.TargetType, isImplicit);

                    // Act
                    var castResult = TypeHelper.CastImplicitlyTo(value, testCase.TargetType);

                    // Assert
                    var succeeded = castResult.IsSuccessful == generatedTestSuccessful;
                    return succeeded;
                }, isImplicitCastable: isImplicit);
        }

        ////[Fact]
        ////public void ShouldSucceedIsCastableTo()
        ////{
        ////    this.RunTests((from, to) => from.IsCastableTo(to), isImplicitCastable: false);
        ////}


        [Fact]
        public void ShouldCastToExplicitly()
        {
            TypeHelper.IsCacheEnabled = false;
            bool isImplicit = false;

            this.RunTests(
                (testCase) =>
                {
                    // Arrange
                    var value = this.GenerateValueForType(testCase.SourceType);
                    var generatedTestSuccessful = this.CastValueWithGeneratedCode(value, testCase.SourceType, testCase.TargetType, isImplicit);

                    // Act
                    var castResult = TypeHelper.CastTo(value, testCase.TargetType);

                    // Assert
                    var succeeded = castResult.IsSuccessful == generatedTestSuccessful;
                    return succeeded;
                }, isImplicitCastable: isImplicit);
        }

        [Fact]
        public void ShouldGenerateValueForEachConsideredType()
        {
            // Arrange
            var allTypesToConsider = GetAllTestTypes().ToList();
            var values = new List<object>(allTypesToConsider.Count);

            // Act
            foreach (var type in allTypesToConsider)
            {
                var value = this.GenerateValueForType(type);
                this.output.WriteLine("Type: {0}, Value: {1}", type.GetFormattedName(), value);
                values.Add(value);
            }

            // Assert
            values.Should().HaveCount(allTypesToConsider.Count);
        }

        private void RunTests(Func<CompilerConversionTestCase, bool> isCastableFunc, bool isImplicitCastable)
        {
            var allTypesToConsider = GetAllTestTypes();

            var testCases = this.GenerateTestCases(allTypesToConsider, isImplicitCastable);

            var mistakes = new List<string>();
            foreach (var testCase in testCases.Where(tc => tc.IsCompilable))
            {
                var isSuccess = isCastableFunc(testCase);
                if (!isSuccess)
                {
                    ////Debugger.Launch();
                    mistakes.Add(
                        string.Format("{0} => {1}: failed to {2} cast",
                        testCase.SourceType.GetFormattedName(),
                        testCase.TargetType.GetFormattedName(),
                        isImplicitCastable ? "implicitly" : "explicitly"));
                }
            }
            Assert.True(mistakes.Count == 0, string.Join(Environment.NewLine, new[] { mistakes.Count + " errors" }.Concat(mistakes)));
        }

        private object GenerateValueForType(Type type)
        {
            if (type == typeof(object))
            {
                return new object();
            }
            if (type == typeof(bool))
            {
                return true;
            }
            if (type == typeof(bool?))
            {
                return true;
            }
            if (type == typeof(byte) || type == typeof(byte?))
            {
                return new byte();
            }
            if (type == typeof(char) || type == typeof(char?))
            {
                return 'c';
            }
            if (type == typeof(double))
            {
                return 1.234d;
            }
            if (type == typeof(double?))
            {
                return new double?(1.234d);
            }
            if (type == typeof(Single))
            {
                return (Single)1234;
            }
            if (type == typeof(Single?))
            {
                return new Single?(1234);
            }
            if (type == typeof(Int16))
            {
                return (Int16)1234;
            }
            if (type == typeof(Int16?))
            {
                return (Int16?)1234;
            }
            if (type == typeof(UInt16) || type == typeof(UInt16?))
            {
                return (UInt16)1234;
            }
            if (type == typeof(Int32) || type == typeof(Int32?))
            {
                return (Int32)1234;
            }
            if (type == typeof(UInt32) || type == typeof(UInt32?))
            {
                return (UInt32)1234;
            }
            if (type == typeof(Int64) || type == typeof(Int64?))
            {
                return (Int64)1234;
            }
            if (type == typeof(UInt64) || type == typeof(UInt64?))
            {
                return (UInt64)1234;
            }
            if (type == typeof(IntPtr) || type == typeof(IntPtr?))
            {
                return new IntPtr(1234);
            }
            if (type == typeof(UIntPtr) || type == typeof(UIntPtr?))
            {
                return new UIntPtr(1234);
            }
            if (type == typeof(SByte) || type == typeof(SByte?))
            {
                return new SByte();
            }
            if (type == typeof(string))
            {
                return "asdflkj";
            }
            if (type == typeof(decimal) || type == typeof(decimal?))
            {
                return 1.234m;
            }
            if (type == typeof(DateTime) || type == typeof(DateTime?))
            {
                return DateTime.Now;
            }
            if (type == typeof(DateTimeOffset) || type == typeof(DateTimeOffset?))
            {
                return DateTimeOffset.Now;
            }
            if (type == typeof(TimeSpan) || type == typeof(TimeSpan?))
            {
                return new TimeSpan(1, 2, 3, 4);
            }
            if (type == typeof(StringSplitOptions) || type == typeof(StringSplitOptions?))
            {
                return StringSplitOptions.RemoveEmptyEntries;
            }
            if (type == typeof(DateTimeKind) || type == typeof(DateTimeKind?))
            {
                return DateTimeKind.Utc;
            }
            if (type == typeof(OperatorsStruct))
            {
                return new OperatorsStruct();
            }
            if (type == typeof(OperatorsStruct?))
            {
                return new OperatorsStruct();
            }
            if (type == typeof(Operators))
            {
                return new Operators();
            }
            if (type == typeof(Operators2))
            {
                return new Operators2();
            }
            if (type == typeof(DerivedOperators))
            {
                return new DerivedOperators();
            }
            if (type == typeof(PrivateOperators))
            {
                return new PrivateOperators();
            }
            if (type == typeof(string[]))
            {
                return new[] { "a", "b" };
            }
            if (type == typeof(object[]))
            {
                return new[] { new object(), new object() };
            }
            if (type == typeof(IEnumerable<string>))
            {
                return new List<string>();
            }
            if (type == typeof(IEnumerable<object>))
            {
                return new List<object>();
            }
            if (type == typeof(Func<string>))
            {
                return new Func<string>(() => "");
            }
            if (type == typeof(Func<object>))
            {
                return new Func<object>(() => new object());
            }
            if (type == typeof(Action<string>))
            {
                return new Action<string>(x => { });
            }
            if (type == typeof(Action<object>))
            {
                return new Action<object>(x => { });
            }

            throw new InvalidOperationException(string.Format("Could not generate an instance of type {0}. Please register.", type.GetFormattedName()));
        }

        private static IEnumerable<Type> GetAllTestTypes()
        {
            var primitives = typeof(object).GetTypeInfo().Assembly.DefinedTypes.Where(t => t.IsPrimitive).ToArray();
            var simpleTypes = new[]
            {
                typeof(string),
                typeof(DateTime), 
                typeof(decimal),
                typeof(object), 
                typeof(DateTimeOffset), 
                typeof(TimeSpan),
                typeof(StringSplitOptions), 
                typeof(DateTimeKind)
            };
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
            var conversionOperators = new[]
            {
                typeof(Operators), 
                typeof(Operators2), 
                typeof(DerivedOperators), 
                typeof(OperatorsStruct)
            };
            var typesToConsider = primitives.Concat(simpleTypes)
                                            .Concat(variantTypes)
                                            .Concat(conversionOperators).ToArray();
            var nullableTypes = typesToConsider.Where(t => t.IsValueType).Select(t => typeof(Nullable<>).MakeGenericType(t));
            var allTypesToConsider = typesToConsider.Concat(nullableTypes);

            return allTypesToConsider;
        }

        [DebuggerDisplay("SourceType = {SourceType}, TargetType = {TargetType}, IsCompilable = {IsCompilable}")]
        private class CompilerConversionTestCase
        {
            public CompilerConversionTestCase(Type sourceType, Type targetType, string codeline, CompilerError compilerError)
            {
                this.SourceType = sourceType;
                this.TargetType = targetType;
                this.Codeline = codeline;
                this.CompilerError = compilerError;
            }

            public Type SourceType { get; private set; }

            public Type TargetType { get; private set; }

            public string Codeline { get; private set; }

            public CompilerError CompilerError { get; private set; }

            public bool IsCompilable
            {
                get
                {
                    return this.CompilerError == null;
                }
            }
        }

        private static string GetCodeline(int index, Type sourceType, Type targetType, bool isImplicit)
        {
            return string.Format(
                "{0} var{1} = {2}Get<{3}>();",
                targetType.GetFormattedFullname(),
                index,
                isImplicit ? string.Empty : "(" + targetType.GetFormattedFullname() + ")",
                sourceType.GetFormattedFullname());
        }

        private List<CompilerConversionTestCase> GenerateTestCases(IEnumerable<Type> allTypes, bool isImplicit)
        {
            // gather all pairs
            var typeCrossProduct = allTypes.SelectMany(t => allTypes, (sourceType, targetType) => new { sourceType, targetType })
                                           .Select((t, index) => new { t.sourceType, t.targetType, index, codeline = GetCodeline(index, t.sourceType, t.targetType, isImplicit) })
                                           .ToArray();

            // create the code to pass to the compiler
            var code = string.Join(
                Environment.NewLine,
                new[] { "namespace A { public class B { static T Get<T>() { return default(T); } public void C() {" }.Concat(
                    typeCrossProduct.Select(t => t.codeline))
                    .Concat(new[] { "}}}" }));

            // compile the code
            var provider = new CSharpCodeProvider();
            var compilerParams = new CompilerParameters();
            compilerParams.ReferencedAssemblies.Add(this.GetType().Assembly.Location); // reference the current assembly!
            compilerParams.GenerateExecutable = false;
            compilerParams.GenerateInMemory = true;
            var compilationResult = provider.CompileAssemblyFromSource(compilerParams, code);

            // determine the outcome of each conversion by matching compiler errors with conversions by line #
            var testCases = typeCrossProduct.GroupJoin(compilationResult.Errors.Cast<CompilerError>(),
                t => t.index,
                e => e.Line - 2,
                (t, e) => new CompilerConversionTestCase(t.sourceType, t.targetType, t.codeline, e.FirstOrDefault()))
                .ToList();

            // add a special case
            // this can't be verified by the normal means, since it's a private class
            ////testCases.Add(new CompilerConversionTestCase(typeof(PrivateOperators), typeof(int), string.Empty, default(CompilerError)));

            return testCases;
        }


        [Fact]
        public void TestCompiler()
        {
            decimal? value = 1234;
            Operators2 x = (Operators2)value;
        }

        private bool CastValueWithGeneratedCode(object value, Type sourceType, Type targetType, bool isImplicit)
        {
            string className = "GeneratedTestClass";
            string methodName = "RunTest";
            string line = string.Format(
                "{0} x = {1}value;",
                targetType.GetFormattedFullname(),
                isImplicit ? string.Empty : "(" + targetType.GetFormattedFullname() + ")");

            string code = "public class " + className + "{ public void " + methodName + "(" + sourceType.GetFormattedFullname() + " value) {" + line + "}}";

            using (CSharpCodeProvider provider = new CSharpCodeProvider())
            {
                var compilerParams = new CompilerParameters();
                compilerParams.ReferencedAssemblies.Add(this.GetType().Assembly.Location);
                compilerParams.GenerateExecutable = false;
                compilerParams.GenerateInMemory = true;
                var compilationResult = provider.CompileAssemblyFromSource(compilerParams, code);
                if (compilationResult.Errors.HasErrors)
                {
                    return false;
                }

                var generatedClass = compilationResult.CompiledAssembly.GetType(className);

                var obj = Activator.CreateInstance(generatedClass);
                var testMethod = generatedClass.GetMethod(methodName);

                try
                {
                    testMethod.Invoke(obj, new[] { value });
                    return true;
                }
                catch (Exception ex)
                {
                    this.output.WriteLine(ex.ToString());
                    return false;
                }
            }
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