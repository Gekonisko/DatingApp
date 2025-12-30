using API.Entities;
using API.Extensions;
using API.Interfaces;
using FluentAssertions;
using Moq;

namespace Api.Tests.UnitTests.Extensions;

public class AppUserExtensionsTests
{
    [Fact]
    public async Task ToDto_Should_Map_User_Properties_And_Generate_Token()
    {
        // Arrange
        var user = new AppUser
        {
            Id = "user-id",
            DisplayName = "Test User",
            Email = "test@test.com",
            ImageUrl = "image.jpg"
        };

        var tokenServiceMock = new Mock<ITokenService>();
        tokenServiceMock
            .Setup(x => x.CreateToken(user))
            .ReturnsAsync("fake-jwt-token");

        // Act
        var dto = await user.ToDto(tokenServiceMock.Object);

        // Assert
        dto.Should().NotBeNull();
        dto.Id.Should().Be(user.Id);
        dto.DisplayName.Should().Be(user.DisplayName);
        dto.Email.Should().Be(user.Email);
        dto.ImageUrl.Should().Be(user.ImageUrl);
        dto.Token.Should().Be("fake-jwt-token");

        tokenServiceMock.Verify(
            x => x.CreateToken(user),
            Times.Once);
    }
}