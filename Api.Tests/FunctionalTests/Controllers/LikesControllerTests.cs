using API.Data;
using API.Entities;
using API.Helpers;
using API.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;

namespace Api.Tests.FunctionalTests.Controllers
{
    public class LikesControllerTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly CustomWebApplicationFactory _factory;

        public LikesControllerTests(CustomWebApplicationFactory factory)
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
                UserName = email
            };

            var result = await userManager.CreateAsync(user, "Pa$$w0rd123");

            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            // Ensure Member exists and is linked to the created AppUser
            var createdUser = await userManager.FindByEmailAsync(email);
            if (createdUser != null)
            {
                var existingMember = await db.Members.FindAsync(createdUser.Id);
                if (existingMember == null)
                {
                    var member = new Member
                    {
                        Id = createdUser.Id,
                        DisplayName = "Test User",
                        Gender = "male",
                        City = "City",
                        Country = "Country",
                        DateOfBirth = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-25))
                    };
                    db.Members.Add(member);
                    await db.SaveChangesAsync();
                }
            }

            // Return token for the created user
            var tokenUser = await userManager.FindByEmailAsync(email);
            return await tokenService.CreateToken(tokenUser!);
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
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var targetUser = await db.Users.Include(u => u.Member).SingleAsync(u => u.Email == "target@test.com");

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
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var targetUser = await db.Users.Include(u => u.Member).SingleAsync(u => u.Email == "user2@test.com");

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
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var targetUser = await db.Users.Include(u => u.Member).SingleAsync(u => u.Email == "userB@test.com");

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
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var user = await db.Users.Include(u => u.Member).SingleAsync(u => u.Email == "self@test.com");

            // Act
            var response = await client.PostAsync($"/api/likes/{user.Member.Id}", null);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }
    }
}