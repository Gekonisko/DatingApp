using API.Extensions;
using FluentAssertions;
using System.Security.Claims;

namespace Api.Tests.UnitTests.Extensions;

public class ClaimsPrincipalExtensionsTests
{
    [Fact]
    public void GetMemberId_Should_Return_NameIdentifier_Claim()
    {
        // Arrange
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "member-123")
        };

        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);

        // Act
        var memberId = principal.GetMemberId();

        // Assert
        memberId.Should().Be("member-123");
    }

    [Fact]
    public void GetMemberId_Should_Throw_When_Claim_Missing()
    {
        // Arrange
        var identity = new ClaimsIdentity(); // no claims
        var principal = new ClaimsPrincipal(identity);

        // Act
        Action act = () => principal.GetMemberId();

        // Assert
        act.Should()
            .Throw<Exception>()
            .WithMessage("Cannot get memberId from token");
    }
}