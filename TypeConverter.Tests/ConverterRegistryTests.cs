using System;
using System.Collections.Generic;

using FluentAssertions;

using TypeConverter.Converters;
using TypeConverter.Exceptions;
using TypeConverter.Tests.Stubs;
using TypeConverter.Tests.Utils;
using TypeConverter.Utils;

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
            TypeHelper.IsCacheEnabled = false;
        }

        #region IConverter Tests

        [Fact]
        public void ShouldThrowConversionNotSupportedExceptionWhenTryingToConvertWithoutValidRegistration()
        {
            // Arrange
            const string InputString = "http://www.google.com/";
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
            const string InputString = "http://www.google.com/";
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
            const string InputString = "http://www.google.com/";
            IConverterRegistry converterRegistry = new ConverterRegistry();
            converterRegistry.RegisterConverter<Uri, string>(() => new StringToUriConverter());

            // Act
            Action action = () => converterRegistry.Convert(typeof(string), typeof(Uri), InputString);

            // Assert
            Assert.Throws<ConversionNotSupportedException>(action);
        }

        [Fact]
        public void ShouldConvertUsingConverterType()
        {
            // Arrange
            const string InputString = "http://www.google.com/";
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
            const string InputString = "http://www.google.com/";
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
            const string InputString = "http://www.google.com/";
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
            const string InputString = "http://www.google.com/";
            IConverterRegistry converterRegistry = new ConverterRegistry();

            // Act
            var convertedObject = converterRegistry.TryConvert<Uri>(InputString);

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
            var convertedObject = converterRegistry.TryConvert<Guid>(testStruct1);

            // Assert
            convertedObject.Should().Be(Guid.Empty);
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
            bool valueType = true;
            IConverterRegistry converterRegistry = new ConverterRegistry();

            // Act
            var nullableValue = converterRegistry.Convert<bool?>(valueType);

            // Assert
            nullableValue.Should().Be(valueType);
        }

        [Fact]
        public void ShouldConvertDoubleToIntegerExplicitly()
        {
            // Arrange
            double doubleValue = 999.99d;
            IConverterRegistry converterRegistry = new ConverterRegistry();

            // Act
            var convertedValue = converterRegistry.Convert<int>(doubleValue);

            // Assert
            convertedValue.Should().Be((int)doubleValue);
        }

        [Fact]
        public void ShouldConvertULongToDecimalImplicitly()
        {
            // Arrange
            ulong ulongValue = 999UL;
            IConverterRegistry converterRegistry = new ConverterRegistry();

            // Act
            var convertedValue = converterRegistry.Convert<decimal>(ulongValue);

            // Assert
            convertedValue.Should().Be(Convert.ToDecimal(ulongValue));
        }

        [Fact]
        public void ShouldRunAllImplicitCasts()
        {
            CastTestRunner.CastFlag castFlag = CastTestRunner.CastFlag.Implicit;
            IConverterRegistry converterRegistry = new ConverterRegistry();

            CastTestRunner.RunTests((testCase) =>
            {
                // Arrange
                var value = CastTestRunner.GenerateValueForType(testCase.SourceType);
                var generatedTestSuccessful = CastTestRunner.CastValueWithGeneratedCode(value, testCase.SourceType, testCase.TargetType, castFlag);

                // Act
                var convertedObject = converterRegistry.TryConvert(testCase.SourceType, testCase.TargetType, value);

                // Assert
                var castResult = new CastResult(convertedObject);
                var isSuccessful = CastTestRunner.AreEqual(
                       this.testOutputHelper,
                       testCase.SourceType,
                       testCase.TargetType,
                       generatedTestSuccessful,
                       castResult,
                       castFlag);

                return isSuccessful;
            }, castFlag: castFlag);
        }

        [Fact]
        public void ShouldRunAllExplicitCasts()
        {
            //TODO
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

        #region String Parse Tests
        [Fact]
        public void ShouldConvertUsingGenericStringParseMethod()
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
        #endregion

        #region General Tests
        [Fact]
        public void ShouldResetRegistrations()
        {
            // Arrange
            IConverterRegistry converterRegistry = new ConverterRegistry();
            converterRegistry.RegisterConverter<string, Uri>(() => new StringToUriConverter());
            converterRegistry.RegisterConverter<Uri, string>(() => new StringToUriConverter());

            // Act
            converterRegistry.Reset();

            // Assert
            var converterForTypeStringToUri = converterRegistry.GetConverterForType<string, Uri>();
            converterForTypeStringToUri.Should().BeNull();

            var converterForTypeUriToString = converterRegistry.GetConverterForType<Uri, string>();
            converterForTypeUriToString.Should().BeNull();
        }
        #endregion
    }
}