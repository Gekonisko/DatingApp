using API.Data;
using API.Entities;
using API.Helpers;
using FluentAssertions;

namespace Api.Tests.IntegrationTests.Repositories;

public class MessageRepositoryTests : IntegrationTestBase
{
    private MessageRepository CreateRepository() => new(Context);

    private async Task SeedMembersAsync()
    {
        Context.Members.AddRange(
            new Member { Id = "member-1", DisplayName = "Alice", Gender = "male", City = "City", Country = "Country" },
            new Member { Id = "member-2", DisplayName = "Bob", Gender = "male", City = "City", Country = "Country" }
        );

        // Ensure corresponding AppUser rows exist for FK Members(Id) -> AspNetUsers(Id)
        foreach (var id in new[] { "member-1", "member-2" })
        {
            var exists = await Context.Users.FindAsync(id);
            if (exists == null)
            {
                Context.Users.Add(new API.Entities.AppUser
                {
                    Id = id,
                    UserName = id + "@test.local",
                    NormalizedUserName = (id + "@test.local").ToUpperInvariant(),
                    Email = id + "@test.local",
                    NormalizedEmail = (id + "@test.local").ToUpperInvariant(),
                    DisplayName = id
                });
            }
        }

        await Context.SaveChangesAsync();
    }

    [Fact]
    public async Task AddMessage_Should_Persist_Message()
    {
        await SeedMembersAsync();
        var repo = CreateRepository();

        var message = new Message
        {
            Id = Guid.NewGuid().ToString(),
            SenderId = "member-1",
            RecipientId = "member-2",
            Content = "Hello",
            MessageSent = DateTime.UtcNow
        };

        repo.AddMessage(message);
        await Context.SaveChangesAsync();

        var result = await repo.GetMessage(message.Id);
        result.Should().NotBeNull();
        result!.Content.Should().Be("Hello");
    }

    [Fact]
    public async Task DeleteMessage_Should_Remove_Message()
    {
        await SeedMembersAsync();
        var repo = CreateRepository();

        var message = new Message
        {
            Id = "msg-1",
            SenderId = "member-1",
            RecipientId = "member-2",
            Content = "To be deleted",
            MessageSent = DateTime.UtcNow
        };

        Context.Messages.Add(message);
        await Context.SaveChangesAsync();

        repo.DeleteMessage(message);
        await Context.SaveChangesAsync();

        var result = await repo.GetMessage("msg-1");
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetMessage_Should_Return_Message_By_Id()
    {
        await SeedMembersAsync();

        var message = new Message
        {
            Id = "msg-2",
            SenderId = "member-1",
            RecipientId = "member-2",
            Content = "Find me",
            MessageSent = DateTime.UtcNow
        };

        Context.Messages.Add(message);
        await Context.SaveChangesAsync();

        var repo = CreateRepository();
        var result = await repo.GetMessage("msg-2");

        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GetMessagesForMember_Inbox_Should_Return_Received_Messages()
    {
        await SeedMembersAsync();

        Context.Messages.AddRange(
            new Message
            {
                Id = "m1",
                SenderId = "member-2",
                RecipientId = "member-1",
                MessageSent = DateTime.UtcNow,
                Content = "test"
            },
            new Message
            {
                Id = "m2",
                SenderId = "member-1",
                RecipientId = "member-2",
                MessageSent = DateTime.UtcNow,
                Content = "test"
            }
        );

        await Context.SaveChangesAsync();

        var repo = CreateRepository();

        var parameters = new MessageParams
        {
            MemberId = "member-1",
            Container = "Inbox",
            PageNumber = 1,
            PageSize = 10
        };

        var result = await repo.GetMessagesForMember(parameters);

        result.Items.Should().ContainSingle();
        result.Items.First().SenderId.Should().Be("member-2");
    }

    [Fact]
    public async Task GetMessagesForMember_Outbox_Should_Return_Sent_Messages()
    {
        await SeedMembersAsync();

        Context.Messages.AddRange(
            new Message
            {
                Id = "m3",
                SenderId = "member-1",
                RecipientId = "member-2",
                MessageSent = DateTime.UtcNow,
                Content = "test"
            }
        );

        await Context.SaveChangesAsync();

        var repo = CreateRepository();

        var parameters = new MessageParams
        {
            MemberId = "member-1",
            Container = "Outbox",
            PageNumber = 1,
            PageSize = 10
        };

        var result = await repo.GetMessagesForMember(parameters);

        result.Items.Should().ContainSingle();
        result.Items.First().RecipientId.Should().Be("member-2");
    }

    [Fact]
    public async Task GetMessageThread_Should_Return_Ordered_Thread_And_Mark_As_Read()
    {
        await SeedMembersAsync();

        var unreadMessage = new Message
        {
            Id = "thread-1",
            SenderId = "member-2",
            RecipientId = "member-1",
            MessageSent = DateTime.UtcNow.AddMinutes(-10),
            DateRead = null,
            Content = "test"
        };

        Context.Messages.AddRange(
            unreadMessage,
            new Message
            {
                Id = "thread-2",
                SenderId = "member-1",
                RecipientId = "member-2",
                MessageSent = DateTime.UtcNow,
                Content = "test"
            }
        );

        await Context.SaveChangesAsync();

        var repo = CreateRepository();

        var thread = await repo.GetMessageThread("member-1", "member-2");

        thread.Should().HaveCount(2);
        thread.First().Id.Should().Be("thread-1");

        var updated = await repo.GetMessage("thread-1");
        updated!.DateRead.Should().NotBeNull();
    }

    [Fact]
    public async Task AddGroup_Should_Persist_Group()
    {
        var group = new Group("group-1");

        var repo = CreateRepository();
        repo.AddGroup(group);
        await Context.SaveChangesAsync();

        var result = await repo.GetMessageGroup("group-1");
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GetGroupForConnection_Should_Return_Group()
    {
        var group = new Group("group-2")
        {
            Connections =
            {
                new Connection("conn-1", "Alice")
            }
        };

        Context.Groups.Add(group);
        await Context.SaveChangesAsync();

        var repo = CreateRepository();
        var result = await repo.GetGroupForConnection("conn-1");

        result.Should().NotBeNull();
        result!.Name.Should().Be("group-2");
    }

    [Fact]
    public async Task RemoveConnection_Should_Delete_Connection()
    {
        var group = new Group("group-for-conn-2")
        {
            Connections = { new Connection("conn-2", "Bob") }
        };

        Context.Groups.Add(group);
        await Context.SaveChangesAsync();

        var repo = CreateRepository();
        await repo.RemoveConnection("conn-2");

        var result = await repo.GetConnection("conn-2");
        result.Should().BeNull();
    }
}