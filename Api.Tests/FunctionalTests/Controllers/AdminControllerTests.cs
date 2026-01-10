using API.Data;
using API.Entities;
using API.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;

namespace Api.Tests.FunctionalTests.Controllers
{
    public class AdminControllerTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly CustomWebApplicationFactory _factory;

        public AdminControllerTests(CustomWebApplicationFactory factory)
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
                UserName = email
            };

            await userManager.CreateAsync(user, "Pa$$w0rd123");
            if (!string.IsNullOrEmpty(role))
                await userManager.AddToRoleAsync(user, role);

            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var createdUser = await userManager.FindByEmailAsync(email);
            if (createdUser != null)
            {
                var existingMember = await db.Members.FindAsync(createdUser.Id);
                if (existingMember == null)
                {
                    var member = new Member
                    {
                        Id = createdUser.Id,
                        DisplayName = "Admin Test",
                        Gender = "male",
                        City = "City",
                        Country = "Country",
                        DateOfBirth = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-25))
                    };
                    db.Members.Add(member);
                    await db.SaveChangesAsync();
                }
            }

            var tokenService = scope.ServiceProvider.GetRequiredService<ITokenService>();
            var tokenUser = await userManager.FindByEmailAsync(email);
            return await tokenService.CreateToken(tokenUser!);
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
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();

            // Ensure a user (and Member) exists via UserManager so relationship keys match
            var photoUserEmail = "photouser@test.com";
            var photoUser = await userManager.FindByEmailAsync(photoUserEmail);
            if (photoUser == null)
            {
                photoUser = new AppUser
                {
                    DisplayName = "PhotoUser",
                    Email = photoUserEmail,
                    UserName = photoUserEmail,
                    Member = new Member { DisplayName = "PhotoUser", Gender = "male", City = "City", Country = "Country" }
                };
                await userManager.CreateAsync(photoUser, "Pa$$w0rd123");
            }

            // Seed unapproved photo attached to the created member
            var memberId = photoUser.Member.Id;
            var photo = new Photo { Id = 1, MemberId = memberId, Url = "url", IsApproved = false };
            var existingPhoto = await db.Photos.FindAsync(photo.Id);
            if (existingPhoto == null)
            {
                db.Photos.Add(photo);
                await db.SaveChangesAsync();
            }

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
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();

            // Ensure a user (and Member) exists via UserManager so relationship keys match
            var photoUserEmail2 = "photouser2@test.com";
            var photoUser2 = await userManager.FindByEmailAsync(photoUserEmail2);
            if (photoUser2 == null)
            {
                photoUser2 = new AppUser
                {
                    DisplayName = "PhotoUser2",
                    Email = photoUserEmail2,
                    UserName = photoUserEmail2,
                    Member = new Member { DisplayName = "PhotoUser2", Gender = "male", City = "City", Country = "Country" }
                };
                await userManager.CreateAsync(photoUser2, "Pa$$w0rd123");
            }

            var memberId2 = photoUser2.Member.Id;
            var photo = new Photo { Id = 2, MemberId = memberId2, Url = "url2", IsApproved = true };
            var existingPhoto = await db.Photos.FindAsync(photo.Id);
            if (existingPhoto == null)
            {
                db.Photos.Add(photo);
                await db.SaveChangesAsync();
            }

            // Act
            var response = await client.PostAsync("/api/admin/approve-photo/2", null);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var updatedPhoto = await uow.PhotoRepository.GetPhotoById(2);
            updatedPhoto.IsApproved.Should().BeTrue();
        }
    }
}