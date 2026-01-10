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
        var (token1, userId1) = await RegisterAndLoginAsync("alice@test.com");
        var (token2, userId2) = await RegisterAndLoginAsync("bob@test.com");

        var tcs = new TaskCompletionSource<IEnumerable<MessageDto>>();

        var connection = CreateConnection(token1, userId2);

        connection.On<IEnumerable<MessageDto>>("ReceiveMessageThread", messages => tcs.TrySetResult(messages));

        await connection.StartAsync();

        var completed = await Task.WhenAny(tcs.Task, Task.Delay(2000));
        completed.Should().Be(tcs.Task);

        (await tcs.Task).Should().NotBeNull();

        await connection.StopAsync();
    }

    [Fact]
    public async Task SendMessage_Should_Deliver_Message_To_Group()
    {
        var (token1, userId1) = await RegisterAndLoginAsync("sender@test.com");
        var (token2, userId2) = await RegisterAndLoginAsync("recipient@test.com");

        var senderConnection = CreateConnection(token1, userId2);
        var recipientConnection = CreateConnection(token2, userId1);

        var tcs = new TaskCompletionSource<MessageDto>();

        recipientConnection.On<MessageDto>("NewMessage", message => tcs.TrySetResult(message));

        // Start recipient first so it is registered in the hub group
        await recipientConnection.StartAsync();
        await Task.Delay(100);
        await senderConnection.StartAsync();

        await senderConnection.InvokeAsync("SendMessage", new CreateMessageDto { RecipientId = userId2, Content = "Hello" });

        var completed = await Task.WhenAny(tcs.Task, Task.Delay(2000));
        completed.Should().Be(tcs.Task);

        var receivedMessage = await tcs.Task;
        receivedMessage.Should().NotBeNull();
        receivedMessage.Content.Should().Be("Hello");

        await senderConnection.StopAsync();
        await recipientConnection.StopAsync();
    }
}