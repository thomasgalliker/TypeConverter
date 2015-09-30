using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Microsoft.CSharp;

using TypeConverter.Extensions;
using TypeConverter.Tests.Stubs;
using TypeConverter.Utils;

using Xunit;
using Xunit.Abstractions;

namespace TypeConverter.Tests.Utils
{
    internal class CastTestRunner
    {
        internal static void RunTests(Func<CompilerConversionTestCase, bool> runTestCase)
        {
            var allTypesToConsider = GetAllTestTypes();
            var testCasesImplicit = GenerateTestCases(allTypesToConsider, CastFlag.Implicit);
            var testCasesExplicit = GenerateTestCases(allTypesToConsider, CastFlag.Explicit);
            var testCases = testCasesImplicit.Concat(testCasesExplicit);

            var mistakes = new List<string>();
            foreach (var testCase in testCases.Where(tc => tc.IsCompilable))
            {
                var isSuccess = runTestCase(testCase);
                if (!isSuccess)
                {
                    mistakes.Add(
                        string.Format("{0} => {1}: failed to {2} cast",
                        testCase.SourceType.GetFormattedName(),
                        testCase.TargetType.GetFormattedName(),
                        testCase.CastFlag == CastFlag.Explicit ? "implicitly" : "explicitly"));
                }
            }
            Assert.True(mistakes.Count == 0, string.Join(Environment.NewLine, new[] { mistakes.Count + " errors" }.Concat(mistakes)));
        }

        internal static IEnumerable<Type> GetAllTestTypes()
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
                typeof(IOperators), 
                typeof(IGenericOperators<>), 
                typeof(IGenericOperators<string>), 
                typeof(Operators2), 
                typeof(DerivedOperators), 
                typeof(OperatorsStruct)
            };
            var typesToConsider = primitives.Concat(simpleTypes)
                                            .Concat(variantTypes)
                                            .Concat(conversionOperators).ToArray();
            var nullableTypes = typesToConsider.Where(type => type.IsValueType).Select(t => typeof(Nullable<>).MakeGenericType(t));
            var allTypesToConsider = typesToConsider.Concat(nullableTypes);

            return allTypesToConsider;
        }

        private static List<CompilerConversionTestCase> GenerateTestCases(IEnumerable<Type> allTypes, CastFlag castFlag)
        {
        
            // generate cross product of given types
            var typeCrossProduct = allTypes.SelectMany(type => allTypes, (sourceType, targetType) => new { sourceType, targetType })
                                           .Select((type, index) => 
                                               new {
                                                        type.sourceType,
                                                        type.targetType, 
                                                        index, 
                                                        codeline = GetCodeline(index, type.sourceType, type.targetType, castFlag)
                                                    }).ToArray();

            // create the code to pass to the compiler
            var code = string.Join(
                Environment.NewLine,
                new[] { "namespace A { public class B { static T Get<T>() { return default(T); } public void C() {" }.Concat(
                    typeCrossProduct.Select(t => t.codeline))
                    .Concat(new[] { "}}}" }));

            // compile the code
            var provider = new CSharpCodeProvider();
            var compilerParams = new CompilerParameters();
            compilerParams.ReferencedAssemblies.Add(typeof(CastTestRunner).Assembly.Location); // reference the current assembly!
            compilerParams.GenerateExecutable = false;
            compilerParams.GenerateInMemory = true;
            var compilationResult = provider.CompileAssemblyFromSource(compilerParams, code);

            // determine the outcome of each conversion by matching compiler errors with conversions by line #
            var testCases = typeCrossProduct.GroupJoin(compilationResult.Errors.Cast<CompilerError>(),
                type => type.index,
                e => e.Line - 2,
                (type, errors) => new CompilerConversionTestCase(type.sourceType, type.targetType, castFlag, type.codeline, errors.FirstOrDefault()))
                .ToList();

            // add a special case
            // this can't be verified by the normal means, since it's a private class
            ////testCases.Add(new CompilerConversionTestCase(typeof(PrivateOperators), typeof(int), string.Empty, default(CompilerError)));

            return testCases;
        }

        private static string GetCodeline(int index, Type sourceType, Type targetType, CastFlag castFlag)
        {
            return string.Format(
                "{0} var{1} = {2}Get<{3}>();",
                targetType.GetFormattedFullname(),
                index,
                castFlag == CastFlag.Implicit ? string.Empty : "(" + targetType.GetFormattedFullname() + ")",
                sourceType.GetFormattedFullname());
        }

        internal static CastResult CastValueWithGeneratedCode(object value, Type sourceType, Type targetType, CastFlag castFlag)
        {
            string className = "GeneratedTestClass";
            string methodName = "RunTest";
            string castLine = string.Format(
                "{0} x = {1}value; return x;",
                targetType.GetFormattedFullname(),
                castFlag == CastFlag.Implicit ? string.Empty : "(" + targetType.GetFormattedFullname() + ")");

            string code = "public class " + className + " { public " + targetType.GetFormattedFullname() + " " + methodName + "(" + sourceType.GetFormattedFullname() + " value)" +
                          "{" + castLine + "}}";

            using (CSharpCodeProvider provider = new CSharpCodeProvider())
            {
                var compilerParams = new CompilerParameters();
                compilerParams.ReferencedAssemblies.Add(typeof(CastTestRunner).Assembly.Location);
                compilerParams.GenerateExecutable = false;
                compilerParams.GenerateInMemory = true;
                var compilationResult = provider.CompileAssemblyFromSource(compilerParams, code);
                if (compilationResult.Errors.HasErrors)
                {

                    var compilerException = new AggregateException("CastValueWithGeneratedCode failed to generate test class.",
                                                  compilationResult.Errors 
                                                      .OfType<CompilerError>() 
                                                      .Where(e => !e.IsWarning)
                                                      .Select(e => new CompilerException(e.Line, e.Column, e.ErrorText)));
                    return new CastResult(compilerException, castFlag);
                }

                var generatedClass = compilationResult.CompiledAssembly.GetType(className);

                var instance = Activator.CreateInstance(generatedClass);
                var testMethod = generatedClass.GetMethod(methodName);

                try
                {
                    var castedValue = testMethod.Invoke(instance, new[] { value });
                    return new CastResult(castedValue, castFlag);
                }
                catch (TargetInvocationException ex)
                {
                    if (ex.InnerException is InvalidProgramException)
                    {
                        // This is most probably an error in Roslyn compiler.
                        // See http://stackoverflow.com/questions/18342943/serious-bugs-with-lifted-nullable-conversions-from-int-allowing-conversion-from
                        return new CastResult(value, castFlag);
                    }

                    return new CastResult(ex, castFlag);
                }
                catch (Exception ex)
                {
                    return new CastResult(ex, castFlag);
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

        internal static object GenerateValueForType(Type type)
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
                return (byte)0x99;
            }
            if (type == typeof(char) || type == typeof(char?))
            {
                return 'c';
            }
            if (type == typeof(double) || type == typeof(double?))
            {
                return 1.234d;
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
                return DateTime.MaxValue;
            }
            if (type == typeof(DateTimeOffset) || type == typeof(DateTimeOffset?))
            {
                return DateTimeOffset.MaxValue;
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
            if (type == typeof(Operators) || type == typeof(IOperators) || type == typeof(IGenericOperators<>) || type == typeof(IGenericOperators<string>))
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

        internal static bool AreEqual(ITestOutputHelper testOutputHelper, Type sourceType, Type targetType, CastResult compilerResult, CastResult castResult, CastFlag expectedCastFlag)
        {
            if (compilerResult.IsSuccessful == true && castResult.IsSuccessful == false)
            {
                // Let's assert the details if the compiler generates a successful result
                // but the CastTo method does not the same.

                var castFlagsAreEqual = compilerResult.CastFlag == castResult.CastFlag || castResult.CastFlag == CastFlag.Implicit;
                if (!castFlagsAreEqual)
                {
                    testOutputHelper.WriteLine("CastFlags of conversion between {0} and {1} are not equal." + Environment.NewLine +
                        "Expected CastFlag: {2}" + Environment.NewLine +
                        "Resulted CastFlag: {3}" + Environment.NewLine,
                        sourceType.GetFormattedName(),
                        targetType.GetFormattedName(),
                        expectedCastFlag,
                        castResult.CastFlag);
                    return false;
                }

                var valuesAreNotEqual = compilerResult.CastFlag == castResult.CastFlag && !Equals(compilerResult.Value, castResult.Value);
                if (valuesAreNotEqual)
                {
                    testOutputHelper.WriteLine("Result of {0} conversion between {1} and {2} are not equal.",
                        expectedCastFlag == CastFlag.Implicit ? "implicit" : "explicit",
                        sourceType.GetFormattedName(),
                        targetType.GetFormattedName());

                    return false;
                }
            }

            return true;
        }
    }
}
