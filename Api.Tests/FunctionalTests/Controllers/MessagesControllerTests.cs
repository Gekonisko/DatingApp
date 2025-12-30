using API.DTOs;
using API.Entities;
using API.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace Api.Tests.FunctionalTests.Controllers
{
    public class MessagesControllerTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;

        public MessagesControllerTests(WebApplicationFactory<Program> factory)
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
        public async Task CreateMessage_Should_Return_MessageDto()
        {
            var client = _factory.CreateClient();
            var senderToken = await RegisterAndGetTokenAsync("sender@test.com");
            var recipientToken = await RegisterAndGetTokenAsync("recipient@test.com");

            // Get recipient user id
            using var scope = _factory.Services.CreateScope();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
            var recipient = await userManager.FindByEmailAsync("recipient@test.com");

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
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
            var recipient = await userManager.FindByEmailAsync("recipient2@test.com");
            var sender = await userManager.FindByEmailAsync("sender2@test.com");

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
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
            var recipient = await userManager.FindByEmailAsync("delete_recipient@test.com");
            var sender = await userManager.FindByEmailAsync("delete_sender@test.com");

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