using DevOpsService.Api.Services;
using FluentAssertions;

namespace DevOpsService.UnitTests.Services;

public class JwtServiceTests
{
    [Fact]
    public void IsJwtUnique_NewToken_ReturnsTrue()
    {
        // Arrange
        var service = new JwtService();

        // Act
        var result = service.IsJwtUnique("brand-new-token");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsJwtUnique_AfterMarkingAsUsed_ReturnsFalse()
    {
        // Arrange
        var service = new JwtService();
        service.MarkJwtAsUsed("used-token");

        // Act
        var result = service.IsJwtUnique("used-token");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void MarkJwtAsUsed_DifferentTokens_EachTrackedIndependently()
    {
        // Arrange
        var service = new JwtService();
        service.MarkJwtAsUsed("token-one");

        // Act & Assert
        service.IsJwtUnique("token-one").Should().BeFalse();
        service.IsJwtUnique("token-two").Should().BeTrue();
    }

    [Fact]
    public void IsJwtUnique_EmptyService_AlwaysReturnsTrue()
    {
        // Arrange
        var service = new JwtService();

        // Act & Assert
        service.IsJwtUnique("any-token").Should().BeTrue();
        service.IsJwtUnique("another-token").Should().BeTrue();
    }
}