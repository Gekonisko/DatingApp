using FluentAssertions;
using Microsoft.AspNetCore.SignalR.Client;

namespace Api.Tests.IntegrationTests.SignalR;

public class PresenceHubTests : SignalRTestBase
{
    public PresenceHubTests(SignalRWebApplicationFactory factory)
        : base(factory) { }

    [Fact]
    public async Task OnConnectedAsync_Should_Add_User_To_Online_List()
    {
        var token = await RegisterAndLoginAsync("online@test.com");

        List<string>? onlineUsers = null;

        var connection = new HubConnectionBuilder()
            .WithUrl($"{Factory.Server.BaseAddress}hubs/presence", options =>
            {
                options.AccessTokenProvider = () => Task.FromResult(token);
            })
            .WithAutomaticReconnect()
            .Build();

        connection.On<List<string>>(
            "GetOnlineUsers",
            users => onlineUsers = users);

        await connection.StartAsync();
        await Task.Delay(300);

        onlineUsers.Should().NotBeNull();
        onlineUsers!.Count.Should().Be(1);

        await connection.StopAsync();
    }

    [Fact]
    public async Task OnConnectedAsync_Should_Notify_Others_User_Online()
    {
        var token1 = await RegisterAndLoginAsync("user1@test.com");
        var token2 = await RegisterAndLoginAsync("user2@test.com");

        string? onlineUserId = null;

        var connection1 = CreatePresenceConnection(token1);
        var connection2 = CreatePresenceConnection(token2);

        connection1.On<string>(
            "UserOnline",
            userId => onlineUserId = userId);

        await connection1.StartAsync();
        await connection2.StartAsync();
        await Task.Delay(300);

        onlineUserId.Should().NotBeNull();

        await connection1.StopAsync();
        await connection2.StopAsync();
    }

    [Fact]
    public async Task OnDisconnectedAsync_Should_Remove_User_From_Online_List()
    {
        var token = await RegisterAndLoginAsync("offline@test.com");

        List<string>? onlineUsers = null;

        var connection = CreatePresenceConnection(token);

        connection.On<List<string>>(
            "GetOnlineUsers",
            users => onlineUsers = users);

        await connection.StartAsync();
        await Task.Delay(200);

        await connection.StopAsync();
        await Task.Delay(300);

        onlineUsers.Should().NotBeNull();
        onlineUsers!.Should().BeEmpty();
    }

    [Fact]
    public async Task OnDisconnectedAsync_Should_Notify_Others_User_Offline()
    {
        var token1 = await RegisterAndLoginAsync("online1@test.com");
        var token2 = await RegisterAndLoginAsync("online2@test.com");

        string? offlineUserId = null;

        var connection1 = CreatePresenceConnection(token1);
        var connection2 = CreatePresenceConnection(token2);

        connection1.On<string>(
            "UserOffline",
            userId => offlineUserId = userId);

        await connection1.StartAsync();
        await connection2.StartAsync();

        await connection2.StopAsync();
        await Task.Delay(300);

        offlineUserId.Should().NotBeNull();

        await connection1.StopAsync();
    }

    private HubConnection CreatePresenceConnection(string token)
    {
        return new HubConnectionBuilder()
            .WithUrl($"{Factory.Server.BaseAddress}hubs/presence", options =>
            {
                options.AccessTokenProvider = () => Task.FromResult(token);
            })
            .WithAutomaticReconnect()
            .Build();
    }
}