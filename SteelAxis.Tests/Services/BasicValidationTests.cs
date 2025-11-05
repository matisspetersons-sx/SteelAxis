using Xunit;

namespace SteelAxis.Tests.Services;

/// <summary>
/// Basic validation tests to ensure test infrastructure is working
/// </summary>
public class BasicValidationTests
{
    [Fact]
    public void TestInfrastructure_IsWorking()
    {
        // Arrange
        var expected = 42;
        
        // Act
        var actual = 40 + 2;
        
        // Assert
        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData("Free")]
    [InlineData("Basic")]
    [InlineData("Professional")]
    [InlineData("Enterprise")]
    public void SubscriptionTiers_AreValid(string tier)
    {
        // Arrange
        var validTiers = new[] { "Free", "Basic", "Professional", "Enterprise" };
        
        // Act & Assert
        Assert.Contains(tier, validTiers);
    }

    [Theory]
    [InlineData("Admin")]
    [InlineData("Manager")]
    [InlineData("User")]
    public void UserRoles_AreValid(string role)
    {
        // Arrange
        var validRoles = new[] { "Admin", "Manager", "User" };
        
        // Act & Assert
        Assert.Contains(role, validRoles);
    }
}
