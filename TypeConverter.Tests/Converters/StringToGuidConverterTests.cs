using System;

using FluentAssertions;

using TypeConverter.Converters;

using Xunit;

namespace TypeConverter.Tests.Converters
{
    public class StringToGuidConverterTests
    {
        [Fact]
        public void ShouldConvertFormatBStringToGuid()
        {
            // Arrange
            const string InputString = "{1E20D9BB-D64C-4449-AC1B-36CB690601ED}";
            IConverterRegistry converterRegistry = new ConverterRegistry();
            converterRegistry.RegisterConverter<string, Guid>(() => new StringToGuidConverter());

            // Act
            var outputGuid = converterRegistry.Convert<string, Guid>(InputString);

            // Assert
            outputGuid.Should().Be(new Guid(InputString));
        }

        [Fact]
        public void ShouldConvertFormatNStringToGuid()
        {
            // Arrange
            const string InputString = "4568CA6400E742BAAA41E76916DE7118";
            IConverterRegistry converterRegistry = new ConverterRegistry();
            converterRegistry.RegisterConverter<string, Guid>(() => new StringToGuidConverter());

            // Act
            var outputGuid = converterRegistry.Convert<string, Guid>(InputString);

            // Assert
            outputGuid.Should().Be(new Guid(InputString));
        }

        [Fact]
        public void ShouldConvertGuidToBFormatString()
        {
            // Arrange
            var inputGuid = new Guid("83EDDA8A-4538-4BA8-8D40-E82C561CD745");
            IConverterRegistry converterRegistry = new ConverterRegistry();
            converterRegistry.RegisterConverter<Guid,string>(() => new StringToGuidConverter());

            // Act
            var outputString = converterRegistry.Convert<Guid, string>(inputGuid);

            // Assert
            outputString.Should().Be("{83edda8a-4538-4ba8-8d40-e82c561cd745}");
        }
    }
}