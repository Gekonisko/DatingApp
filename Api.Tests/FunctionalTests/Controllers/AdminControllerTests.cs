using API.Entities;
using API.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;

namespace Api.Tests.FunctionalTests.Controllers
{
    public class AdminControllerTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;

        public AdminControllerTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
        }

        private async Task<string> RegisterAndGetTokenAsync(string email, string role = null)
        {
            using var scope = _factory.Services.CreateScope();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();

            var user = new AppUser
            {
                DisplayName = "Admin Test",
                Email = email,
                UserName = email,
                Member = new Member { DisplayName = "Admin Test", Gender = "male", City = "City", Country = "Country" }
            };

            await userManager.CreateAsync(user, "Pa$$w0rd123");
            if (!string.IsNullOrEmpty(role))
                await userManager.AddToRoleAsync(user, role);

            var tokenService = scope.ServiceProvider.GetRequiredService<ITokenService>();
            return await tokenService.CreateToken(user);
        }

        [Fact]
        public async Task GetUsersWithRoles_Should_Return_Users_When_Admin()
        {
            // Arrange
            var client = _factory.CreateClient();
            var token = await RegisterAndGetTokenAsync("admin@test.com", "Admin");

            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

            // Act
            var response = await client.GetAsync("/api/admin/users-with-roles");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var users = await response.Content.ReadFromJsonAsync<List<dynamic>>();
            users.Should().NotBeNull();
            users.Count.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task EditRoles_Should_Add_Remove_Roles()
        {
            // Arrange
            var client = _factory.CreateClient();
            var token = await RegisterAndGetTokenAsync("admin2@test.com", "Admin");

            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

            // Create a normal user
            var userToken = await RegisterAndGetTokenAsync("user@test.com");

            using var scope = _factory.Services.CreateScope();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
            var user = await userManager.FindByEmailAsync("user@test.com");

            // Act
            var response = await client.PostAsync($"/api/admin/edit-roles/{user.Id}?roles=Member,Moderator", null);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var roles = await response.Content.ReadFromJsonAsync<List<string>>();
            roles.Should().Contain(new[] { "Member", "Moderator" });
        }

        [Fact]
        public async Task GetPhotosForModeration_Should_Return_Unapproved_Photos()
        {
            // Arrange
            var client = _factory.CreateClient();
            var token = await RegisterAndGetTokenAsync("moderator@test.com", "Moderator");

            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

            using var scope = _factory.Services.CreateScope();
            var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

            // Seed unapproved photo
            var member = new Member { DisplayName = "PhotoUser", Id = "member1", Gender = "male", City = "City", Country = "Country" };
            var photo = new Photo { Id = 1, MemberId = member.Id, Url = "url", IsApproved = false };
            uow.MemberRepository.Update(member);
            uow.PhotoRepository.RemovePhoto(photo); // ensure no duplicates
            await uow.Complete();

            // Act
            var response = await client.GetAsync("/api/admin/photos-to-moderate");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var photos = await response.Content.ReadFromJsonAsync<List<Photo>>();
            photos.Should().ContainSingle(p => p.Id == 1);
        }

        [Fact]
        public async Task ApprovePhoto_Should_Set_IsApproved_True()
        {
            // Arrange
            var client = _factory.CreateClient();
            var token = await RegisterAndGetTokenAsync("moderator2@test.com", "Moderator");

            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

            using var scope = _factory.Services.CreateScope();
            var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

            var member = new Member { DisplayName = "PhotoUser2", Id = "member2", Gender = "male", City = "City", Country = "Country" };
            var photo = new Photo { Id = 2, MemberId = member.Id, Url = "url2", IsApproved = false };
            uow.MemberRepository.Update(member);
            await uow.Complete();

            // Act
            var response = await client.PostAsync("/api/admin/approve-photo/2", null);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var updatedPhoto = await uow.PhotoRepository.GetPhotoById(2);
            updatedPhoto.IsApproved.Should().BeTrue();
        }
    }
}