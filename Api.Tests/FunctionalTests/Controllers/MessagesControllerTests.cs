using API.Data;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace Api.Tests.FunctionalTests.Controllers
{
    public class MessagesControllerTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly CustomWebApplicationFactory _factory;

        public MessagesControllerTests(CustomWebApplicationFactory factory)
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
        public async Task CreateMessage_Should_Return_MessageDto()
        {
            var client = _factory.CreateClient();
            var senderToken = await RegisterAndGetTokenAsync("sender@test.com");
            var recipientToken = await RegisterAndGetTokenAsync("recipient@test.com");

            // Get recipient user id
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var recipient = await db.Users.Include(u => u.Member).SingleAsync(u => u.Email == "recipient@test.com");

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", senderToken);

            var createMessageDto = new CreateMessageDto
            {
                RecipientId = recipient.Member.Id,
                Content = "Hello!"
            };

            var response = await client.PostAsJsonAsync("/api/messages", createMessageDto);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var messageDto = await response.Content.ReadFromJsonAsync<MessageDto>();
            messageDto.Content.Should().Be("Hello!");
            messageDto.SenderId.Should().NotBeNull();
            messageDto.RecipientId.Should().Be(recipient.Member.Id);
        }

        [Fact]
        public async Task GetMessageThread_Should_Return_List()
        {
            var client = _factory.CreateClient();
            var senderToken = await RegisterAndGetTokenAsync("sender2@test.com");
            var recipientToken = await RegisterAndGetTokenAsync("recipient2@test.com");

            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var recipient = await db.Users.Include(u => u.Member).SingleAsync(u => u.Email == "recipient2@test.com");
            var sender = await db.Users.Include(u => u.Member).SingleAsync(u => u.Email == "sender2@test.com");

            // Seed a message
            var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var message = new Message
            {
                SenderId = sender.Member.Id,
                RecipientId = recipient.Member.Id,
                Content = "Test thread"
            };
            uow.MessageRepository.AddMessage(message);
            await uow.Complete();

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", senderToken);

            var response = await client.GetAsync($"/api/messages/thread/{recipient.Member.Id}");
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var thread = await response.Content.ReadFromJsonAsync<IReadOnlyList<MessageDto>>();
            thread.Should().NotBeEmpty();
            thread[0].Content.Should().Be("Test thread");
        }

        [Fact]
        public async Task DeleteMessage_Should_Return_Ok_When_SenderDeletes()
        {
            var client = _factory.CreateClient();
            var senderToken = await RegisterAndGetTokenAsync("delete_sender@test.com");
            var recipientToken = await RegisterAndGetTokenAsync("delete_recipient@test.com");

            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var recipient = await db.Users.Include(u => u.Member).SingleAsync(u => u.Email == "delete_recipient@test.com");
            var sender = await db.Users.Include(u => u.Member).SingleAsync(u => u.Email == "delete_sender@test.com");

            var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var message = new Message
            {
                SenderId = sender.Member.Id,
                RecipientId = recipient.Member.Id,
                Content = "Message to delete"
            };
            uow.MessageRepository.AddMessage(message);
            await uow.Complete();

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", senderToken);

            var response = await client.DeleteAsync($"/api/messages/{message.Id}");
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }
    }
}