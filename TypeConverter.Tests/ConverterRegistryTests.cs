using System;
using System.Collections.Generic;

using FluentAssertions;
using TypeConverter.Converters;
using TypeConverter.Exceptions;
using TypeConverter.Tests.Stubs;
using TypeConverter.Tests.Utils;

using Xunit;
using Xunit.Abstractions;

namespace TypeConverter.Tests
{
    public class ConverterRegistryTests
    {
        private readonly ITestOutputHelper testOutputHelper;

        public ConverterRegistryTests(ITestOutputHelper testOutputHelper)
        {
            this.testOutputHelper = testOutputHelper;
        }

        #region IConvertable Tests

        [Fact]
        public void ShouldThrowConversionNotSupportedExceptionWhenTryingToConvertWithoutValidRegistration()
        {
            // Arrange
            const string InputString = "http://www.superdev.ch/";
            IConverterRegistry converterRegistry = new ConverterRegistry();

            // Act
            Action action = () => converterRegistry.Convert(typeof(string), typeof(Uri), InputString);

            // Assert
            Assert.Throws<ConversionNotSupportedException>(action);
        }

        [Fact]
        public void ShouldThrowConversionNotSupportedExceptionWhenTryingToConvertGenericWithoutValidRegistration()
        {
            // Arrange
            const string InputString = "http://www.superdev.ch/";
            IConverterRegistry converterRegistry = new ConverterRegistry();

            // Act
            Action action = () => converterRegistry.Convert<string, Uri>(InputString);

            // Assert
            Assert.Throws<ConversionNotSupportedException>(action);
        }

        [Fact]
        public void ShouldThrowConversionNotSupportedExceptionWhenWrongConversionWayIsConfigured()
        {
            // Arrange
            const string InputString = "http://www.superdev.ch/";
            IConverterRegistry converterRegistry = new ConverterRegistry();
            converterRegistry.RegisterConverter<Uri, string>(() => new StringToUriConverter());

            // Act
            Action action = () => converterRegistry.Convert(typeof(string), typeof(Uri), InputString);

            // Assert
            Assert.Throws<ConversionNotSupportedException>(action);
        }

        [Fact]
        public void ShouldRegisterConverterImplicitly()
        {
            // Arrange
            const string InputString = "http://www.superdev.ch/";
            var stringToUriConverter = new StringToUriConverter();
            IConverterRegistry converterRegistry = new ConverterRegistry();
            converterRegistry.RegisterConverter(stringToUriConverter);

            // Act
            var convertedObject = converterRegistry.Convert<string, Uri>(InputString);
            var outputString = converterRegistry.Convert<Uri, string>(convertedObject);

            // Assert
            convertedObject.Should().NotBeNull();
            convertedObject.Should().BeOfType<Uri>();
            convertedObject.As<Uri>().AbsoluteUri.Should().Be(InputString);

            outputString.Should().NotBeNullOrEmpty();
            outputString.Should().Be(InputString);
        }

        [Fact]
        public void ShouldConvertUsingConverterType()
        {
            // Arrange
            const string InputString = "http://www.superdev.ch/";
            Type converterType = typeof(StringToUriConverter);
            IConverterRegistry converterRegistry = new ConverterRegistry();
            converterRegistry.RegisterConverter<string, Uri>(converterType);
            converterRegistry.RegisterConverter<Uri, string>(converterType);

            // Act
            var convertedObject = converterRegistry.Convert<string, Uri>(InputString);
            var outputString = converterRegistry.Convert<Uri, string>(convertedObject);

            // Assert
            convertedObject.Should().NotBeNull();
            convertedObject.Should().BeOfType<Uri>();
            convertedObject.As<Uri>().AbsoluteUri.Should().Be(InputString);

            outputString.Should().NotBeNullOrEmpty();
            outputString.Should().Be(InputString);
        }

        [Fact]
        public void ShouldConvertUsingGenericSourceTypeAndNongenericTargetType()
        {
            // Arrange
            const string InputString = "http://www.superdev.ch/";
            Type converterType = typeof(StringToUriConverter);
            IConverterRegistry converterRegistry = new ConverterRegistry();
            converterRegistry.RegisterConverter<string, Uri>(converterType);
            converterRegistry.RegisterConverter<Uri, string>(converterType);

            // Act
            var convertedObject = (Uri)converterRegistry.Convert(typeof(Uri), InputString);
            var outputString = converterRegistry.Convert(typeof(string), convertedObject);

            // Assert
            convertedObject.Should().NotBeNull();
            convertedObject.Should().BeOfType<Uri>();
            convertedObject.As<Uri>().AbsoluteUri.Should().Be(InputString);

            outputString.Should().NotBeNull();
            outputString.Should().Be(InputString);
        }

        [Fact]
        public void ShouldConvertUsingGenericTargetTypeAndObjectSourceType()
        {
            // Arrange
            const string InputString = "http://www.superdev.ch/";
            Type converterType = typeof(StringToUriConverter);
            IConverterRegistry converterRegistry = new ConverterRegistry();
            converterRegistry.RegisterConverter<string, Uri>(converterType);
            converterRegistry.RegisterConverter<Uri, string>(converterType);

            // Act
            var convertedObject = (Uri)converterRegistry.Convert<Uri>(InputString);
            var outputString = converterRegistry.Convert<string>(convertedObject);

            // Assert
            convertedObject.Should().NotBeNull();
            convertedObject.Should().BeOfType<Uri>();
            convertedObject.As<Uri>().AbsoluteUri.Should().Be(InputString);

            outputString.Should().NotBeNull();
            outputString.Should().Be(InputString);
        }

        [Fact]
        public void ShouldReturnDefaultValueWhenTryConvertToReferenceTypeFails()
        {
            // Arrange
            const string InputString = "http://www.superdev.ch/";
            IConverterRegistry converterRegistry = new ConverterRegistry();

            // Act
            var convertedObject = converterRegistry.TryConvert<Uri>(InputString, null);

            // Assert
            convertedObject.Should().BeNull();
        }

        [Fact]
        public void ShouldReturnDefaultValueWhenTryConvertToValueTypeFails()
        {
            // Arrange
            TestStruct1 testStruct1 = new TestStruct1 { TestString = Guid.NewGuid().ToString() };
            IConverterRegistry converterRegistry = new ConverterRegistry();

            // Act
            var convertedObject = converterRegistry.TryConvert<Guid>(testStruct1, default(Guid));

            // Assert
            convertedObject.Should().Be(Guid.Empty);
        }

        [Fact]
        public void ShouldTryConvertEnumImplicitlyWithGenericMethod()
        {
            // Arrange
            object sourceObject = MyEnum.TestValue;
            const MyEnum DefaultValue = default(MyEnum);
            IConverterRegistry converterRegistry = new ConverterRegistry();

            // Act
            MyEnum convertedObject = converterRegistry.TryConvert(sourceObject, DefaultValue);

            // Assert
            convertedObject.Should().Be(sourceObject);
        }

        [Fact]
        public void ShouldTryConvertEnumImplicitlyWithNonGenericMethod()
        {
            // Arrange
            object sourceObject = MyEnum.TestValue;
            const MyEnum DefaultValue = default(MyEnum);
            IConverterRegistry converterRegistry = new ConverterRegistry();

            // Act
            object convertedObject = converterRegistry.TryConvert(typeof(object), typeof(MyEnum), sourceObject, DefaultValue);

            // Assert
            convertedObject.Should().Be(sourceObject);
        }

        #endregion

        #region Implicit and explicit cast tests

        [Fact]
        public void ShouldConvertIfSourceTypeIsEqualToTargetType()
        {
            // Arrange
            const string InputString = "999";
            IConverterRegistry converterRegistry = new ConverterRegistry();

            // Act
            var convertedObject = (string)converterRegistry.Convert(typeof(string), typeof(string), InputString);

            // Assert
            convertedObject.Should().Be(InputString);
        }

        [Fact]
        public void ShouldConvertIfTargetTypeIsAssignableFromSourceType()
        {
            // Arrange
            List<string> stringList = new List<string> { "a", "b", "c" };
            IConverterRegistry converterRegistry = new ConverterRegistry();

            // Act
            var convertedList = (IEnumerable<string>)converterRegistry.Convert(typeof(IEnumerable<string>), stringList);

            // Assert
            convertedList.Should().BeEquivalentTo(stringList);
        }

        [Fact]
        public void ShouldConvertEnumerableToArray()
        {
            // Arrange
            string[] stringArray = { "a", "b", "c" };
            IConverterRegistry converterRegistry = new ConverterRegistry();

            // Act
            var convertedList = (IEnumerable<string>)converterRegistry.Convert(typeof(IEnumerable<string>), stringArray);

            // Assert
            convertedList.Should().BeEquivalentTo(stringArray);
        }

        [Fact]
        public void ShouldThrowConversionNotSupportedExceptionWhenTryingToConvertArrayToEnumerable()
        {
            // Arrange
            List<string> stringList = new List<string> { "a", "b", "c" };
            IConverterRegistry converterRegistry = new ConverterRegistry();

            // Act
            Action action = () => converterRegistry.Convert(typeof(string[]), stringList);

            // Assert
            Assert.Throws<ConversionNotSupportedException>(action);
        }

        [Fact]
        public void ShouldConvertNullableTypeToValueType()
        {
            // Arrange
            bool? nullableValue = true;
            IConverterRegistry converterRegistry = new ConverterRegistry();

            // Act
            var valueType = converterRegistry.Convert<bool>(nullableValue);

            // Assert
            valueType.Should().Be(nullableValue.Value);
        }

        [Fact]
        public void ShouldConvertValueTypeToNullableType()
        {
            // Arrange
            const bool ValueType = true;
            IConverterRegistry converterRegistry = new ConverterRegistry();

            // Act
            var nullableValue = converterRegistry.Convert<bool?>(ValueType);

            // Assert
            nullableValue.Should().Be(ValueType);
        }

        [Fact]
        public void ShouldConvertDoubleToIntegerExplicitly()
        {
            // Arrange
            const double DoubleValue = 999.99d;
            IConverterRegistry converterRegistry = new ConverterRegistry();

            // Act
            var convertedValue = converterRegistry.Convert<int>(DoubleValue);

            // Assert
            convertedValue.Should().Be((int)DoubleValue);
        }

        [Fact]
        public void ShouldConvertIntegerToDoubleExplicitly()
        {
            // Arrange
            //const double DoubleValue = 0.0d;
            int IntegerValue = 999;
            IConverterRegistry converterRegistry = new ConverterRegistry();
            

            // Act
            var convertedValue = converterRegistry.TryConvert((object)(int)999, (int)0);
            var convertedValue2 = converterRegistry.TryConvert((object)(int)999, (double)0);

            var convertedValue3 = converterRegistry.TryConvert((object)(int)999, (int)0);
            var convertedValue4 = converterRegistry.TryConvert((object)(int)999, (double)0);

            // Assert
            convertedValue.Should().Be(IntegerValue);
        }

        [Fact]
        public void ShouldConvertULongToDecimalImplicitly()
        {
            // Arrange
            const ulong UlongValue = 999UL;
            IConverterRegistry converterRegistry = new ConverterRegistry();

            // Act
            var convertedValue = converterRegistry.Convert<decimal>(UlongValue);

            // Assert
            convertedValue.Should().Be(Convert.ToDecimal(UlongValue));
        }

        [Fact]
        public void ShouldConvertFromOpenGenericTypeToGenericType()
        {
            // Arrange
            IGenericOperators<string> inputValue = new Operators();
            IConverterRegistry converterRegistry = new ConverterRegistry();

            // Act
            var convertedValue = converterRegistry.Convert(typeof(IGenericOperators<>), typeof(IGenericOperators<string>), inputValue);

            // Assert
            convertedValue.Should().Be(inputValue);
        }

        [Fact]
        public void ShouldThrowConversionNotSupportedExceptionWhenTryingToConvertToOpenGenericType()
        {
            // Arrange
            IGenericOperators<string> value = new Operators();
            IConverterRegistry converterRegistry = new ConverterRegistry();

            // Act
            Action action = () => converterRegistry.Convert(typeof(IGenericOperators<string>), typeof(IGenericOperators<>), value);

            // Assert
            Assert.Throws<ConversionNotSupportedException>(action);
        }

        [Fact]
        public void ShouldRunAllDefaultCasts()
        {
            IConverterRegistry converterRegistry = new ConverterRegistry();

            CastTestRunner.RunTests((testCase) =>
            {
                // Arrange
                var value = CastTestRunner.GenerateValueForType(testCase.SourceType);
                var generatedTestSuccessful = CastTestRunner.CastValueWithGeneratedCode(value, testCase.SourceType, testCase.TargetType, testCase.CastFlag);

                // Act
                var convertedObject = converterRegistry.TryConvert(
                    sourceType: testCase.SourceType, 
                    targetType: testCase.TargetType, 
                    value: value, 
                    defaultReturnValue: null);

                // Assert
                var castResult = new CastResult(convertedObject, testCase.CastFlag);
                var isSuccessful = CastTestRunner.AreEqual(
                       this.testOutputHelper,
                       testCase.SourceType,
                       testCase.TargetType,
                       generatedTestSuccessful,
                       castResult,
                       testCase.CastFlag);

                return isSuccessful;
            });
        }

        #endregion

        #region Enum Parse Tests

        [Fact]
        public void ShouldConvertEnumsImplicitly()
        {
            // Arrange
            string inputString = MyEnum.TestValue.ToString();
            IConverterRegistry converterRegistry = new ConverterRegistry();

            // Act
            var convertedObject = (MyEnum)converterRegistry.Convert(typeof(MyEnum), inputString);
            var outputString = converterRegistry.Convert(typeof(string), convertedObject);

            // Assert
            convertedObject.Should().NotBeNull();
            convertedObject.Should().BeOfType<MyEnum>();
            convertedObject.Should().Be(MyEnum.TestValue);

            outputString.Should().NotBeNull();
            outputString.Should().Be(inputString);
        }

        [Fact]
        public void ShouldConvertEnumsImplicitlyWithGenerics()
        {
            // Arrange
            string inputString = MyEnum.TestValue.ToString();
            IConverterRegistry converterRegistry = new ConverterRegistry();

            // Act
            var convertedObject = converterRegistry.Convert<MyEnum>(inputString);
            var outputString = converterRegistry.Convert(typeof(string), convertedObject);

            // Assert
            convertedObject.Should().NotBeNull();
            convertedObject.Should().BeOfType<MyEnum>();
            convertedObject.Should().Be(MyEnum.TestValue);

            outputString.Should().NotBeNull();
            outputString.Should().Be(inputString);
        }

        [Fact]
        public void ShouldConvertEnumsExplicitly()
        {
            // Arrange
            string inputString = MyEnum.TestValue.ToString();
            IConverterRegistry converterRegistry = new ConverterRegistry();
            converterRegistry.RegisterConverter<string, MyEnum>(() => new MyEnumConverter());
            converterRegistry.RegisterConverter<MyEnum, string>(() => new MyEnumConverter());

            // Act
            var convertedObject = (MyEnum)converterRegistry.Convert<MyEnum>(inputString);
            var outputString = converterRegistry.Convert<string>(convertedObject);

            // Assert
            convertedObject.Should().NotBeNull();
            convertedObject.Should().BeOfType<MyEnum>();
            convertedObject.Should().Be(MyEnum.TestValue);

            outputString.Should().NotBeNull();
            outputString.Should().Be(inputString);
        }

        #endregion

        #region ChangeType Tests

        [Fact]
        public void ShouldConvertUsingChangeType()
        {
            // Arrange
            bool? nullableBool = true;
            string valueTypeString = nullableBool.ToString();
            IConverterRegistry converterRegistry = new ConverterRegistry();

            // Act
            var nullableValue = converterRegistry.Convert<bool?>(valueTypeString);

            // Assert
            nullableValue.Should().Be(nullableBool.Value);
        }

        [Fact]
        public void ShouldConvertUsingChangeTypeMethodFromStringToInt()
        {
            // Arrange
            const string InputString = "999";
            IConverterRegistry converterRegistry = new ConverterRegistry();

            // Act
            var convertedObject = (int)converterRegistry.Convert(typeof(int), InputString);
            var outputString = converterRegistry.Convert(typeof(string), convertedObject);

            // Assert
            convertedObject.Should().Be(999);

            outputString.Should().NotBeNull();
            outputString.Should().Be(InputString);
        }

        [Fact]
        public void ShouldConvertUsingChangeTypeMethodFromStringToBool()
        {
            // Arrange
            const string InputString = "True";
            IConverterRegistry converterRegistry = new ConverterRegistry();

            // Act
            var convertedObject = (bool)converterRegistry.Convert(typeof(bool), InputString);
            var outputString = converterRegistry.Convert(typeof(string), convertedObject);

            // Assert
            convertedObject.Should().BeTrue();

            outputString.Should().NotBeNull();
            outputString.Should().Be(InputString);
        }
        #endregion

        #region String Parse Tests
        [Fact]
        public void ShouldConvertUsingStringParse()
        {
            // Arrange
            const string InputString = "http://www.thomasgalliker.ch/";
            Uri inputUri = new Uri(InputString);

            IConverterRegistry converterRegistry = new ConverterRegistry();

            // Act
            var uriAsString = converterRegistry.Convert<Uri, string>(inputUri);

            // Assert
            uriAsString.Should().Be(InputString);
        }
        #endregion

        #region General Tests
        [Fact]
        public void ShouldResetRegistrations()
        {
            // Arrange
            const string InputString = "http://www.thomasgalliker.ch";
            int numberOfConvertCalls = 0;
            IConverterRegistry converterRegistry = new ConverterRegistry();
            var converter = new TestConverter(() => { numberOfConvertCalls++; });
            converterRegistry.RegisterConverter<string, Uri>(() => converter);

            // Act
            var convertedInputStringBeforeReset = converterRegistry.TryConvert<string, Uri>(InputString, null);

            converterRegistry.Reset();

            var convertedInputStringAfterReset = converterRegistry.TryConvert<string, Uri>(InputString, null);

            // Assert
            numberOfConvertCalls.Should().Be(1);
            convertedInputStringBeforeReset.Should().Be(InputString);
            convertedInputStringAfterReset.Should().BeNull();
        }

        private class TestConverter : IConvertable<string, Uri>
        {
            private readonly Action convert;

            public TestConverter(Action convert)
            {
                this.convert = convert;
            }

            public Uri Convert(string value)
            {
                this.convert();
                return new Uri(value);
            }
        }
        #endregion
    }
}