using DevOpsService.Api.Service;
using FluentAssertions;

namespace DevOpsService.UnitTests
{
    public class JwtServiceTests
    {
        [Fact]
        public void IsJwtUniqueNewTokenReturnsTrue()
        {
            // Arrange
            JwtService service = new();

            // Act
            bool result = service.IsJwtUnique("brand-new-token");

            // Assert
            _ = result.Should().BeTrue();
        }

        [Fact]
        public void IsJwtUniqueAfterMarkingAsUsedReturnsFalse()
        {
            // Arrange
            JwtService service = new();
            service.MarkJwtAsUsed("used-token");

            // Act
            bool result = service.IsJwtUnique("used-token");

            // Assert
            _ = result.Should().BeFalse();
        }

        [Fact]
        public void MarkJwtAsUsedDifferentTokensEachTrackedIndependently()
        {
            // Arrange
            JwtService service = new();
            service.MarkJwtAsUsed("token-one");

            // Act & Assert
            _ = service.IsJwtUnique("token-one").Should().BeFalse();
            _ = service.IsJwtUnique("token-two").Should().BeTrue();
        }

        [Fact]
        public void IsJwtUniqueEmptyServiceAlwaysReturnsTrue()
        {
            // Arrange
            JwtService service = new();

            // Act & Assert
            _ = service.IsJwtUnique("any-token").Should().BeTrue();
            _ = service.IsJwtUnique("another-token").Should().BeTrue();
        }
    }
}