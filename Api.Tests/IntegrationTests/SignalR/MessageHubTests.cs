using API.DTOs;
using FluentAssertions;
using Microsoft.AspNetCore.SignalR.Client;

namespace Api.Tests.IntegrationTests.SignalR;

public class MessageHubTests : SignalRTestBase
{
    public MessageHubTests(SignalRWebApplicationFactory factory)
        : base(factory) { }

    [Fact]
    public async Task OnConnectedAsync_Should_Send_Message_Thread()
    {
        var token1 = await RegisterAndLoginAsync("alice@test.com");
        var token2 = await RegisterAndLoginAsync("bob@test.com");

        var received = false;

        var connection = CreateConnection(token1, "bob-id");

        connection.On<IEnumerable<MessageDto>>(
            "ReceiveMessageThread",
            messages => received = true);

        await connection.StartAsync();

        received.Should().BeTrue();

        await connection.StopAsync();
    }

    [Fact]
    public async Task SendMessage_Should_Deliver_Message_To_Group()
    {
        var token1 = await RegisterAndLoginAsync("sender@test.com");
        var token2 = await RegisterAndLoginAsync("recipient@test.com");

        var senderConnection = CreateConnection(token1, "recipient-id");
        var recipientConnection = CreateConnection(token2, "sender-id");

        MessageDto? receivedMessage = null;

        recipientConnection.On<MessageDto>(
            "NewMessage",
            message => receivedMessage = message);

        await senderConnection.StartAsync();
        await recipientConnection.StartAsync();

        await senderConnection.InvokeAsync(
            "SendMessage",
            new CreateMessageDto
            {
                RecipientId = "recipient-id",
                Content = "Hello"
            });

        await Task.Delay(500);

        receivedMessage.Should().NotBeNull();
        receivedMessage!.Content.Should().Be("Hello");

        await senderConnection.StopAsync();
        await recipientConnection.StopAsync();
    }
}