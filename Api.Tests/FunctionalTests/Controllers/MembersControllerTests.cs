using API.Data;
using API.DTOs;
using API.Entities;
using API.Helpers;
using API.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace Api.Tests.FunctionalTests.Controllers
{
    public class MembersControllerTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly CustomWebApplicationFactory _factory;

        public MembersControllerTests(CustomWebApplicationFactory factory)
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

            await userManager.CreateAsync(user, "Pa$$w0rd123");

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

            var tokenUser = await userManager.FindByEmailAsync(email);
            return await tokenService.CreateToken(tokenUser!);
        }

        [Fact]
        public async Task GetMembers_Should_Return_List()
        {
            var client = _factory.CreateClient();
            var token = await RegisterAndGetTokenAsync("member1@test.com");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Create another member so the returned list is not empty
            await RegisterAndGetTokenAsync("member_other@test.com");

            var response = await client.GetAsync("/api/members");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<PaginatedResult<Member>>();
            result.Should().NotBeNull();
            result!.Items.Should().NotBeEmpty();
        }

        [Fact]
        public async Task GetMember_Should_Return_Single_Member()
        {
            var client = _factory.CreateClient();
            var token = await RegisterAndGetTokenAsync("member2@test.com");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var member = await db.Users.Include(u => u.Member).SingleAsync(u => u.Email == "member2@test.com");

            var response = await client.GetAsync($"/api/members/{member.Member.Id}");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<Member>();
            result.Id.Should().Be(member.Member.Id);
        }

        [Fact]
        public async Task UpdateMember_Should_Return_NoContent()
        {
            var client = _factory.CreateClient();
            var token = await RegisterAndGetTokenAsync("member3@test.com");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var updateDto = new MemberUpdateDto
            {
                DisplayName = "Updated Name",
                City = "Updated City"
            };

            var response = await client.PutAsJsonAsync("/api/members", updateDto);

            response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }

        [Fact]
        public async Task AddPhoto_Should_Return_Photo()
        {
            var client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    var photoServiceMock = new Mock<IPhotoService>();
                    photoServiceMock.Setup(x => x.UploadPhotoAsync(It.IsAny<IFormFile>()))
                        .ReturnsAsync(new CloudinaryDotNet.Actions.ImageUploadResult
                        {
                            SecureUrl = new Uri("http://test.com/photo.jpg"),
                            PublicId = "test-id"
                        });

                    services.AddSingleton(photoServiceMock.Object);
                });
            }).CreateClient();

            var token = await RegisterAndGetTokenAsync("member4@test.com");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var fileContent = new MultipartFormDataContent();
            var bytes = new byte[10];
            fileContent.Add(new ByteArrayContent(bytes), "file", "test.jpg");

            var response = await client.PostAsync("/api/members/add-photo", fileContent);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }
    }
}