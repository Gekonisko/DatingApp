using API.DTOs;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;

namespace Api.Tests.FunctionalTests.Controllers
{
    public class AccountControllerTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;

        public AccountControllerTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
        }

        private RegisterDto GetTestRegisterDto(string email) => new()
        {
            DisplayName = "Test User",
            Email = email,
            Password = "Pa$$w0rd123",
            City = "Test City",
            Country = "Test Country",
            Gender = "male",
            DateOfBirth = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-25))
        };

        private LoginDto GetTestLoginDto(string email) => new()
        {
            Email = email,
            Password = "Pa$$w0rd123"
        };

        [Fact]
        public async Task Register_Should_Return_UserDto()
        {
            // Arrange
            var client = _factory.CreateClient();
            var email = "register@test.com";
            var registerDto = GetTestRegisterDto(email);

            // Act
            var response = await client.PostAsJsonAsync("/api/account/register", registerDto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var user = await response.Content.ReadFromJsonAsync<UserDto>();
            user.Should().NotBeNull();
            user!.Email.Should().Be(email);
            user.DisplayName.Should().Be(registerDto.DisplayName);
            user.Token.Should().NotBeNullOrWhiteSpace();
        }

        [Fact]
        public async Task Login_Should_Return_UserDto()
        {
            // Arrange
            var client = _factory.CreateClient();
            var email = "login@test.com";

            // First register
            var registerDto = GetTestRegisterDto(email);
            await client.PostAsJsonAsync("/api/account/register", registerDto);

            // Act
            var loginDto = GetTestLoginDto(email);
            var response = await client.PostAsJsonAsync("/api/account/login", loginDto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var user = await response.Content.ReadFromJsonAsync<UserDto>();
            user.Should().NotBeNull();
            user!.Email.Should().Be(email);
            user.Token.Should().NotBeNullOrWhiteSpace();
        }

        [Fact]
        public async Task RefreshToken_Should_Return_UserDto()
        {
            // Arrange
            var client = _factory.CreateClient();
            var email = "refresh@test.com";
            var registerDto = GetTestRegisterDto(email);

            // Register user
            await client.PostAsJsonAsync("/api/account/register", registerDto);

            // Act
            var response = await client.PostAsync("/api/account/refresh-token", null);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var user = await response.Content.ReadFromJsonAsync<UserDto>();
            user.Should().NotBeNull();
            user!.Email.Should().Be(email);
            user.Token.Should().NotBeNullOrWhiteSpace();
        }

        [Fact]
        public async Task Logout_Should_Clear_RefreshToken()
        {
            // Arrange
            var client = _factory.CreateClient();
            var email = "logout@test.com";
            var registerDto = GetTestRegisterDto(email);

            // Register
            var registerResponse = await client.PostAsJsonAsync("/api/account/register", registerDto);
            var user = await registerResponse.Content.ReadFromJsonAsync<UserDto>();

            // Add token cookie manually for auth
            client.DefaultRequestHeaders.Add("Cookie", $"refreshToken={user!.Token}");

            // Act
            var response = await client.PostAsync("/api/account/logout", null);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }
    }
}