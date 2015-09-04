
using System;

using FluentAssertions;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using TypeConverter.Exceptions;
using TypeConverter.Tests.Testdata;

namespace TypeConverter.Tests
{
    [TestClass]
    public class TypeConverterTests
    {
        [TestMethod]
        [ExpectedException(typeof(ConversionNotSupportedException))]
        public void ShouldThrowConversionNotSupportedExceptionWhenTryingToConvertWithoutValidRegistration()
        {
            // Arrange
            const string InputString = "http://www.google.com/";
            IConverterRegistry converterRegistry = new ConverterRegistry();

            // Act
            object convertedObject = converterRegistry.Convert(typeof(string), typeof(Uri), InputString);

            // Assert
        }

        [TestMethod]
        public void ShouldConvertOneWay()
        {
            // Arrange
            const string InputString = "http://www.google.com/";
            IConverterRegistry converterRegistry = new ConverterRegistry();
            converterRegistry.RegisterConverter<string, Uri>(() => new StringToUriConverter());

            // Act
            object convertedObject = converterRegistry.Convert(typeof(string), typeof(Uri), InputString);

            // Assert
            convertedObject.Should().NotBeNull();
            convertedObject.Should().BeOfType<Uri>();
            convertedObject.As<Uri>().AbsoluteUri.Should().Be(InputString);
        }

        [TestMethod]
        [ExpectedException(typeof(ConversionNotSupportedException))]
        public void ShouldNotConvertIfRegistrationIsMissingEvenIfConverterIsImplementingBothWayConversion()
        {
            // Arrange
            const string InputString = "http://www.google.com/";
            IConverterRegistry converterRegistry = new ConverterRegistry();
            converterRegistry.RegisterConverter<Uri, string>(() => new StringToUriConverter());

            // Act
            object convertedObject = converterRegistry.Convert(typeof(string), typeof(Uri), InputString);
        }

        [TestMethod]
        public void ShouldConvertBothWays()
        {
            // Arrange
            const string InputString = "http://www.google.com/";
            IConverterRegistry converterRegistry = new ConverterRegistry();
            converterRegistry.RegisterConverter<string, Uri>(() => new StringToUriConverter());
            converterRegistry.RegisterConverter<Uri, string>(() => new StringToUriConverter());

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


        [TestMethod]
        public void ShouldConvertBothWaysUsingConverterType()
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

        [TestMethod]
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

        [TestMethod]
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

        [TestMethod]
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
    }
}
