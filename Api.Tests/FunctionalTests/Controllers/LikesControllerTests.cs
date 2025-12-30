using API.Entities;
using API.Helpers;
using API.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;

namespace Api.Tests.FunctionalTests.Controllers
{
    public class LikesControllerTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;

        public LikesControllerTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
        }

        private async Task<string> RegisterAndGetTokenAsync(string email)
        {
            using var scope = _factory.Services.CreateScope();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
            var tokenService = scope.ServiceProvider.GetRequiredService<ITokenService>();

            var user = new AppUser
            {
                DisplayName = "Test User",
                Email = email,
                UserName = email,
                Member = new Member { DisplayName = "Test User", Gender = "male", City = "City", Country = "Country" }
            };

            await userManager.CreateAsync(user, "Pa$$w0rd123");

            return await tokenService.CreateToken(user);
        }

        [Fact]
        public async Task ToggleLike_Should_Add_And_Remove_Like()
        {
            // Arrange
            var client = _factory.CreateClient();
            var token = await RegisterAndGetTokenAsync("source@test.com");
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

            var targetToken = await RegisterAndGetTokenAsync("target@test.com");
            using var scope = _factory.Services.CreateScope();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
            var targetUser = await userManager.FindByEmailAsync("target@test.com");

            // Act - Add like
            var responseAdd = await client.PostAsync($"/api/likes/{targetUser.Member.Id}", null);

            // Assert - Add
            responseAdd.StatusCode.Should().Be(HttpStatusCode.OK);

            // Act - Remove like
            var responseRemove = await client.PostAsync($"/api/likes/{targetUser.Member.Id}", null);

            // Assert - Remove
            responseRemove.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task GetCurrentMemberLikeIds_Should_Return_Ids()
        {
            // Arrange
            var client = _factory.CreateClient();
            var token = await RegisterAndGetTokenAsync("user1@test.com");
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

            var targetToken = await RegisterAndGetTokenAsync("user2@test.com");
            using var scope = _factory.Services.CreateScope();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
            var targetUser = await userManager.FindByEmailAsync("user2@test.com");

            // Add like first
            await client.PostAsync($"/api/likes/{targetUser.Member.Id}", null);

            // Act
            var response = await client.GetAsync("/api/likes/list");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var ids = await response.Content.ReadFromJsonAsync<IReadOnlyList<string>>();
            ids.Should().Contain(targetUser.Member.Id);
        }

        [Fact]
        public async Task GetMemberLikes_Should_Return_PaginatedResult()
        {
            // Arrange
            var client = _factory.CreateClient();
            var token = await RegisterAndGetTokenAsync("userA@test.com");
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

            var targetToken = await RegisterAndGetTokenAsync("userB@test.com");
            using var scope = _factory.Services.CreateScope();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
            var targetUser = await userManager.FindByEmailAsync("userB@test.com");

            // Add like
            await client.PostAsync($"/api/likes/{targetUser.Member.Id}", null);

            // Act
            var response = await client.GetAsync("/api/likes?pageNumber=1&pageSize=10&predicate=liked");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<PaginatedResult<Member>>();
            result.Items.Should().ContainSingle(x => x.Id == targetUser.Member.Id);
        }

        [Fact]
        public async Task ToggleLike_Should_Return_BadRequest_When_Same_User()
        {
            // Arrange
            var client = _factory.CreateClient();
            var token = await RegisterAndGetTokenAsync("self@test.com");
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

            using var scope = _factory.Services.CreateScope();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
            var user = await userManager.FindByEmailAsync("self@test.com");

            // Act
            var response = await client.PostAsync($"/api/likes/{user.Member.Id}", null);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }
    }
}