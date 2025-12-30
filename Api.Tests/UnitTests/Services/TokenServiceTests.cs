using API.Entities;
using API.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Moq;
using System.IdentityModel.Tokens.Jwt;

namespace Api.Tests.UnitTests.Services;

public class TokenServiceTests
{
    private readonly Mock<UserManager<AppUser>> _userManagerMock;
    private readonly IConfiguration _config;

    public TokenServiceTests()
    {
        _userManagerMock = MockUserManager();

        var configValues = new Dictionary<string, string?>
        {
            ["TokenKey"] = new string('x', 64)
        };

        _config = new ConfigurationBuilder()
            .AddInMemoryCollection(configValues!)
            .Build();
    }

    [Fact]
    public async Task CreateToken_Should_Return_Valid_Jwt()
    {
        // Arrange
        var user = new AppUser
        {
            Id = "user-id",
            Email = "test@test.com",
            DisplayName = "DisplayName"
        };

        _userManagerMock
            .Setup(x => x.GetRolesAsync(user))
            .ReturnsAsync(new List<string>());

        var tokenService = new TokenService(_config, _userManagerMock.Object);

        // Act
        var token = await tokenService.CreateToken(user);

        // Assert
        token.Should().NotBeNullOrWhiteSpace();

        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);

        jwt.Should().NotBeNull();
    }

    [Fact]
    public async Task CreateToken_Should_Contain_Email_And_UserId_Claims()
    {
        // Arrange
        var user = new AppUser
        {
            Id = "user-id",
            Email = "test@test.com",
            DisplayName = "DisplayName"
        };

        _userManagerMock
            .Setup(x => x.GetRolesAsync(user))
            .ReturnsAsync(new List<string>());

        var tokenService = new TokenService(_config, _userManagerMock.Object);

        // Act
        var token = await tokenService.CreateToken(user);
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

        // Assert
        jwt.Claims.Should().Contain(c =>
            c.Type == "email" && c.Value == user.Email);

        jwt.Claims.Should().Contain(c =>
            c.Type == "nameid" && c.Value == user.Id);
    }

    [Fact]
    public async Task CreateToken_Should_Include_Role_Claims()
    {
        // Arrange
        var user = new AppUser
        {
            Id = "user-id",
            Email = "test@test.com",
            DisplayName = "DisplayName"
        };

        _userManagerMock
            .Setup(x => x.GetRolesAsync(user))
            .ReturnsAsync(new List<string> { "Admin", "Member" });

        var tokenService = new TokenService(_config, _userManagerMock.Object);

        // Act
        var token = await tokenService.CreateToken(user);
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

        // Assert
        jwt.Claims.Should().Contain(c => c.Type == "role" && c.Value == "Admin");
        jwt.Claims.Should().Contain(c => c.Type == "role" && c.Value == "Member");
    }

    [Fact]
    public async Task CreateToken_Should_Throw_When_TokenKey_Missing()
    {
        // Arrange
        var badConfig = new ConfigurationBuilder().Build();
        var tokenService = new TokenService(badConfig, _userManagerMock.Object);

        // Act
        Func<Task> act = async () =>
            await tokenService.CreateToken(new AppUser() { DisplayName = "DisplayName" });

        // Assert
        await act.Should()
            .ThrowAsync<Exception>()
            .WithMessage("Cannot get token key");
    }

    [Fact]
    public async Task CreateToken_Should_Throw_When_TokenKey_Too_Short()
    {
        // Arrange
        var shortKeyConfig = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["TokenKey"] = "short-key"
            })
            .Build();

        var tokenService = new TokenService(shortKeyConfig, _userManagerMock.Object);

        // Act
        Func<Task> act = async () =>
            await tokenService.CreateToken(new AppUser() { DisplayName = "DisplayName" });

        // Assert
        await act.Should()
            .ThrowAsync<Exception>()
            .WithMessage("Your token key needs to be >= 64 characters");
    }

    [Fact]
    public void GenerateRefreshToken_Should_Return_Random_Base64_String()
    {
        // Arrange
        var tokenService = new TokenService(_config, _userManagerMock.Object);

        // Act
        var token1 = tokenService.GenerateRefreshToken();
        var token2 = tokenService.GenerateRefreshToken();

        // Assert
        token1.Should().NotBeNullOrWhiteSpace();
        token2.Should().NotBeNullOrWhiteSpace();
        token1.Should().NotBe(token2);

        // 64 bytes -> Base64 length = 88 chars
        token1.Length.Should().Be(88);
    }

    private static Mock<UserManager<AppUser>> MockUserManager()
    {
        var store = new Mock<IUserStore<AppUser>>();

        return new Mock<UserManager<AppUser>>(
            store.Object,
            null!, null!, null!, null!, null!, null!, null!, null!);
    }
}