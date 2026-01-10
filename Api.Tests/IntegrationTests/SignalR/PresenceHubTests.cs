using FluentAssertions;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.SignalR.Client;

namespace Api.Tests.IntegrationTests.SignalR;

public class PresenceHubTests : SignalRTestBase
{
    public PresenceHubTests(SignalRWebApplicationFactory factory)
        : base(factory) { }

    [Fact]
    public async Task OnConnectedAsync_Should_Add_User_To_Online_List()
    {
        var (token, userId) = await RegisterAndLoginAsync("online@test.com");

        var tcs = new TaskCompletionSource<List<string>>();

        var connection = CreatePresenceConnection(token);

        connection.On<List<string>>("GetOnlineUsers", users => tcs.TrySetResult(users));

        await connection.StartAsync();

        var completed = await Task.WhenAny(tcs.Task, Task.Delay(2000));
        completed.Should().Be(tcs.Task);

        var onlineUsers = await tcs.Task;
        onlineUsers.Should().NotBeNull();
        onlineUsers.Count.Should().Be(1);

        await connection.StopAsync();
    }

    [Fact]
    public async Task OnConnectedAsync_Should_Notify_Others_User_Online()
    {
        var (token1, userId1) = await RegisterAndLoginAsync("user1@test.com");
        var (token2, userId2) = await RegisterAndLoginAsync("user2@test.com");

        var tcs = new TaskCompletionSource<string>();

        var connection1 = CreatePresenceConnection(token1);
        var connection2 = CreatePresenceConnection(token2);

        connection1.On<string>("UserOnline", userId => tcs.TrySetResult(userId));

        await connection1.StartAsync();
        await connection2.StartAsync();

        var completed = await Task.WhenAny(tcs.Task, Task.Delay(2000));
        completed.Should().Be(tcs.Task);

        (await tcs.Task).Should().NotBeNull();

        await connection1.StopAsync();
        await connection2.StopAsync();
    }

    [Fact]
    public async Task OnDisconnectedAsync_Should_Remove_User_From_Online_List()
    {
        var (token, userId) = await RegisterAndLoginAsync("offline@test.com");

        var tcs = new TaskCompletionSource<List<string>>();

        var connection = CreatePresenceConnection(token);

        connection.On<List<string>>("GetOnlineUsers", users => tcs.TrySetResult(users));

        await connection.StartAsync();

        // Wait for initial online list
        var initialCompleted = await Task.WhenAny(tcs.Task, Task.Delay(2000));
        initialCompleted.Should().Be(tcs.Task);

        await connection.StopAsync();
        // Allow server time to process OnDisconnectedAsync and update presence
        await Task.Delay(200);

        // After disconnect, have a different observer connect and request online users
        var (observerToken, observerId) = await RegisterAndLoginAsync("observer@test.com");

        var tcsAfter = new TaskCompletionSource<List<string>>();
        var connection2 = CreatePresenceConnection(observerToken);
        connection2.On<List<string>>("GetOnlineUsers", users => tcsAfter.TrySetResult(users));
        await connection2.StartAsync();

        var afterCompleted = await Task.WhenAny(tcsAfter.Task, Task.Delay(2000));
        afterCompleted.Should().Be(tcsAfter.Task);

        var onlineUsersAfter = await tcsAfter.Task;
        onlineUsersAfter.Should().NotContain(userId);

        await connection2.StopAsync();
    }

    [Fact]
    public async Task OnDisconnectedAsync_Should_Notify_Others_User_Offline()
    {
        var (token1, userId1) = await RegisterAndLoginAsync("online1@test.com");
        var (token2, userId2) = await RegisterAndLoginAsync("online2@test.com");

        var tcs = new TaskCompletionSource<string>();

        var connection1 = CreatePresenceConnection(token1);
        var connection2 = CreatePresenceConnection(token2);

        connection1.On<string>("UserOffline", userId => tcs.TrySetResult(userId));

        await connection1.StartAsync();
        await connection2.StartAsync();

        await connection2.StopAsync();

        var completed = await Task.WhenAny(tcs.Task, Task.Delay(2000));
        completed.Should().Be(tcs.Task);

        (await tcs.Task).Should().NotBeNull();

        await connection1.StopAsync();
    }

    private HubConnection CreatePresenceConnection(string token)
    {
        var hubUrl = new Uri(Factory.Server.BaseAddress, "hubs/presence");

        return new HubConnectionBuilder()
            .WithUrl(hubUrl, options =>
            {
                options.AccessTokenProvider = () => Task.FromResult<string?>(token);
                options.Transports = HttpTransportType.LongPolling;
                options.HttpMessageHandlerFactory = _ => Factory.Server.CreateHandler();
            })
            .WithAutomaticReconnect()
            .Build();
    }
}