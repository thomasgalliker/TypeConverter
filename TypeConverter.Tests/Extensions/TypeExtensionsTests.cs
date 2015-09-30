using System.Linq;
using System.Reflection;

using FluentAssertions;

using TypeConverter.Extensions;
using TypeConverter.Tests.Stubs;

using Xunit;

namespace TypeConverter.Tests.Extensions
{
    public class TypeExtensionsTests
    {
        [Fact]
        public void ShouldReturnAllMethodsOfType()
        {
            // Arrange
            var type = typeof(Operators);

            // Act
            var declaredMethods = type.GetDeclaredMethodsRecursively();

            // Assert
            declaredMethods.Should().HaveCount(17);
            declaredMethods.Count(x => x.Name == "op_Implicit").Should().Be(2);
            declaredMethods.Count(x => x.Name == "op_Explicit").Should().Be(3);
        }

        [Fact]
        public void ShouldReturnAllMethodsOfDerivedType()
        {
            // Arrange
            var type = typeof(DerivedOperators);

            // Act
            var declaredMethods = type.GetDeclaredMethodsRecursively();

            // Assert
            declaredMethods.Should().HaveCount(20);
            declaredMethods.Count(x => x.Name == "op_Implicit").Should().Be(2);
            declaredMethods.Count(x => x.Name == "op_Explicit").Should().Be(6);
        }

        [Fact]
        public void ShouldReturnAllMethodsOfDerivedTypeInfo()
        {
            // Arrange
            var typeInfo = typeof(DerivedOperators).GetTypeInfo();

            // Act
            var declaredMethods = typeInfo.GetDeclaredMethodsRecursively();

            // Assert
            declaredMethods.Should().HaveCount(20);
            declaredMethods.Count(x => x.Name == "op_Implicit").Should().Be(2);
            declaredMethods.Count(x => x.Name == "op_Explicit").Should().Be(6);
        }

        [Fact]
        public void IsSameOrParentShouldDetectSameType()
        {
            // Arrange
            var childTypeInfo = typeof(Operators).GetTypeInfo();
            var parentTypeInfo = typeof(Operators).GetTypeInfo();

            // Act
            var isSameOrParent = parentTypeInfo.IsSameOrParent(childTypeInfo);

            // Assert
            isSameOrParent.Should().BeTrue();
        }

        [Fact]
        public void IsSameOrParentShouldDetectParent()
        {
            // Arrange
            var childTypeInfo = typeof(DerivedOperators).GetTypeInfo();
            var parentTypeInfo = typeof(Operators).GetTypeInfo();

            // Act
            var isSameOrParent = parentTypeInfo.IsSameOrParent(childTypeInfo);

            // Assert
            isSameOrParent.Should().BeTrue();
        }
    }
}