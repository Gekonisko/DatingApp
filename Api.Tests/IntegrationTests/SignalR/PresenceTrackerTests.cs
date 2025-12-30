using API.SignalR;
using FluentAssertions;

namespace Api.Tests.UnitTests.SignalR;

public class PresenceTrackerTests : IDisposable
{
    private readonly PresenceTracker _tracker = new();

    public PresenceTrackerTests()
    {
        ClearTracker();
    }

    [Fact]
    public async Task UserConnected_Should_Add_User_And_Connection()
    {
        await _tracker.UserConnected("user1", "conn1");

        var users = await _tracker.GetOnlineUsers();

        users.Should().ContainSingle()
             .Which.Should().Be("user1");

        var connections = await PresenceTracker.GetConnectionsForUser("user1");
        connections.Should().ContainSingle()
                   .Which.Should().Be("conn1");
    }

    [Fact]
    public async Task UserConnected_Should_Allow_Multiple_Connections()
    {
        await _tracker.UserConnected("user1", "conn1");
        await _tracker.UserConnected("user1", "conn2");

        var connections = await PresenceTracker.GetConnectionsForUser("user1");

        connections.Should().HaveCount(2);
        connections.Should().Contain(new[] { "conn1", "conn2" });
    }

    [Fact]
    public async Task UserDisconnected_Should_Remove_Single_Connection()
    {
        await _tracker.UserConnected("user1", "conn1");
        await _tracker.UserConnected("user1", "conn2");

        await _tracker.UserDisconnected("user1", "conn1");

        var connections = await PresenceTracker.GetConnectionsForUser("user1");

        connections.Should().ContainSingle()
                   .Which.Should().Be("conn2");
    }

    [Fact]
    public async Task UserDisconnected_Should_Remove_User_When_Last_Connection_Removed()
    {
        await _tracker.UserConnected("user1", "conn1");

        await _tracker.UserDisconnected("user1", "conn1");

        var users = await _tracker.GetOnlineUsers();

        users.Should().BeEmpty();
    }

    [Fact]
    public async Task GetOnlineUsers_Should_Return_Sorted_UserIds()
    {
        await _tracker.UserConnected("z-user", "c1");
        await _tracker.UserConnected("a-user", "c2");
        await _tracker.UserConnected("m-user", "c3");

        var users = await _tracker.GetOnlineUsers();

        users.Should().Equal("a-user", "m-user", "z-user");
    }

    [Fact]
    public async Task GetConnectionsForUser_Should_Return_Empty_List_When_User_Offline()
    {
        var connections = await PresenceTracker.GetConnectionsForUser("missing");

        connections.Should().BeEmpty();
    }

    public void Dispose()
    {
        ClearTracker();
    }

    private static void ClearTracker()
    {
        var field = typeof(PresenceTracker)
            .GetField("OnlineUsers",
                System.Reflection.BindingFlags.Static |
                System.Reflection.BindingFlags.NonPublic);

        var dictionary = (System.Collections.IDictionary?)field?.GetValue(null);
        dictionary?.Clear();
    }
}