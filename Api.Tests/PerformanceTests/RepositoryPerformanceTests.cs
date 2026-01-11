using Api.Tests.FunctionalTests;
using API.Data;
using API.Entities;
using API.Helpers;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit.Abstractions;

namespace Api.Tests.PerformanceTests;

[Collection("Performance")]
public class RepositoryPerformanceTests : PerformanceTestBase
{
    private readonly AppDbContext _context;
    private readonly MemberRepository _memberRepository;
    private readonly MessageRepository _messageRepository;
    private readonly LikesRepository _likesRepository;
    private readonly List<string> _memberIds = new();

    public RepositoryPerformanceTests(ITestOutputHelper output) : base(output)
    {
        var factory = new CustomWebApplicationFactory();
        var scope = factory.Services.CreateScope();
        _context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        _memberRepository = new MemberRepository(_context);
        _messageRepository = new MessageRepository(_context);
        _likesRepository = new LikesRepository(_context);

        InitializeDataAsync().Wait();
    }

    private async Task InitializeDataAsync()
    {
        // Create test members
        for (int i = 0; i < 100; i++)
        {
            var member = new Member
            {
                Id = Guid.NewGuid().ToString(),
                DisplayName = $"Member{i}",
                Gender = i % 2 == 0 ? "male" : "female",
                DateOfBirth = new DateOnly(1980 + i % 20, (i % 12) + 1, (i % 28) + 1),
                City = $"City{i % 10}",
                Country = $"Country{i % 5}",
                Description = $"Description for member {i}",
                Created = DateTime.UtcNow.AddDays(-i),
                LastActive = DateTime.UtcNow.AddHours(-i),
                User = new AppUser
                {
                    DisplayName = $"User{i}",
                    Email = $"user{i}@test.com",
                    UserName = $"user{i}@test.com"
                }
            };

            _context.Members.Add(member);
            _memberIds.Add(member.Id);
        }

        await _context.SaveChangesAsync();

        // Create test messages
        for (int i = 0; i < 50; i++)
        {
            var message = new Message
            {
                Id = Guid.NewGuid().ToString(),
                SenderId = _memberIds[i % 20],
                RecipientId = _memberIds[(i + 1) % 20],
                Content = $"Test message {i}",
                MessageSent = DateTime.UtcNow.AddMinutes(-i)
            };
            _context.Messages.Add(message);
        }

        // Create test likes
        for (int i = 0; i < 30; i++)
        {
            var like = new MemberLike
            {
                SourceMemberId = _memberIds[i % 10],
                TargetMemberId = _memberIds[(i + 10) % 30]
            };
            _context.Likes.Add(like);
        }

        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();
    }

    [Fact]
    public async Task MemberRepository_GetMemberById_Performance()
    {
        // Arrange
        var iterations = 200;
        var counter = 0;

        // Act
        var result = await MeasurePerformanceAsync(
            "MemberRepository.GetMemberByIdAsync",
            async () =>
            {
                var index = Interlocked.Increment(ref counter) % _memberIds.Count;
                var member = await _memberRepository.GetMemberByIdAsync(_memberIds[index]);
                member.Should().NotBeNull();
            },
            iterations);

        // Assert
        AssertPerformance(result, maxAverageMs: 50, maxP95Ms: 100);
    }

    [Fact]
    public async Task MemberRepository_GetMembers_WithFilters_Performance()
    {
        // Arrange
        var iterations = 100;

        // Act
        var result = await MeasurePerformanceAsync(
            "MemberRepository.GetMembersAsync with Filters",
            async () =>
            {
                var parameters = new MemberParams
                {
                    CurrentMemberId = _memberIds[0],
                    Gender = "female",
                    MinAge = 18,
                    MaxAge = 40,
                    OrderBy = "lastActive",
                    PageNumber = 1,
                    PageSize = 10
                };

                var members = await _memberRepository.GetMembersAsync(parameters);
                members.Should().NotBeNull();
                members.Items.Should().NotBeEmpty();
            },
            iterations);

        // Assert
        AssertPerformance(result, maxAverageMs: 150, maxP95Ms: 300);
    }

    [Fact]
    public async Task MemberRepository_GetMembers_Pagination_Performance()
    {
        // Arrange
        var iterations = 100;
        var pageCounter = 0;

        // Act
        var result = await MeasurePerformanceAsync(
            "MemberRepository.GetMembersAsync Pagination",
            async () =>
            {
                var page = (Interlocked.Increment(ref pageCounter) % 5) + 1;
                var parameters = new MemberParams
                {
                    CurrentMemberId = _memberIds[0],
                    PageNumber = page,
                    PageSize = 10
                };

                var members = await _memberRepository.GetMembersAsync(parameters);
                members.Should().NotBeNull();
            },
            iterations);

        // Assert
        AssertPerformance(result, maxAverageMs: 100, maxP95Ms: 200);
    }

    [Fact]
    public async Task MemberRepository_GetMemberForUpdate_Performance()
    {
        // Arrange
        var iterations = 100;
        var counter = 0;

        // Act
        var result = await MeasurePerformanceAsync(
            "MemberRepository.GetMemberForUpdate",
            async () =>
            {
                var index = Interlocked.Increment(ref counter) % _memberIds.Count;
                var member = await _memberRepository.GetMemberForUpdate(_memberIds[index]);
                member.Should().NotBeNull();
            },
            iterations);

        // Assert
        AssertPerformance(result, maxAverageMs: 100, maxP95Ms: 200);
    }

    [Fact]
    public async Task MessageRepository_GetMessageThread_Performance()
    {
        // Arrange
        var iterations = 100;

        // Act
        var result = await MeasurePerformanceAsync(
            "MessageRepository.GetMessageThread",
            async () =>
            {
                var messages = await _messageRepository.GetMessageThread(
                    _memberIds[0],
                    _memberIds[1]);
                messages.Should().NotBeNull();
            },
            iterations);

        // Assert
        AssertPerformance(result, maxAverageMs: 150, maxP95Ms: 300);
    }

    [Fact]
    public async Task MessageRepository_GetMessagesForMember_Performance()
    {
        // Arrange
        var iterations = 100;

        // Act
        var result = await MeasurePerformanceAsync(
            "MessageRepository.GetMessagesForMember",
            async () =>
            {
                var parameters = new MessageParams
                {
                    MemberId = _memberIds[0],
                    Container = "Inbox",
                    PageNumber = 1,
                    PageSize = 10
                };

                var messages = await _messageRepository.GetMessagesForMember(parameters);
                messages.Should().NotBeNull();
            },
            iterations);

        // Assert
        AssertPerformance(result, maxAverageMs: 150, maxP95Ms: 300);
    }

    [Fact]
    public async Task MessageRepository_AddAndDelete_Performance()
    {
        // Arrange
        var iterations = 50;

        // Act
        var result = await MeasurePerformanceAsync(
            "MessageRepository Add and Delete",
            async () =>
            {
                var message = new Message
                {
                    Id = Guid.NewGuid().ToString(),
                    SenderId = _memberIds[0],
                    RecipientId = _memberIds[1],
                    Content = "Performance test message",
                    MessageSent = DateTime.UtcNow
                };

                _messageRepository.AddMessage(message);
                await _context.SaveChangesAsync();

                _messageRepository.DeleteMessage(message);
                await _context.SaveChangesAsync();
            },
            iterations);

        // Assert
        AssertPerformance(result, maxAverageMs: 200, maxP95Ms: 400);
    }

    [Fact]
    public async Task LikesRepository_GetMemberLikes_Performance()
    {
        // Arrange
        var iterations = 100;

        // Act
        var result = await MeasurePerformanceAsync(
            "LikesRepository.GetMemberLikes",
            async () =>
            {
                var parameters = new LikesParams
                {
                    MemberId = _memberIds[0],
                    Predicate = "liked",
                    PageNumber = 1,
                    PageSize = 10
                };

                var likes = await _likesRepository.GetMemberLikes(parameters);
                likes.Should().NotBeNull();
            },
            iterations);

        // Assert
        AssertPerformance(result, maxAverageMs: 150, maxP95Ms: 300);
    }

    [Fact]
    public async Task LikesRepository_GetCurrentMemberLikeIds_Performance()
    {
        // Arrange
        var iterations = 150;
        var counter = 0;

        // Act
        var result = await MeasurePerformanceAsync(
            "LikesRepository.GetCurrentMemberLikeIds",
            async () =>
            {
                var index = Interlocked.Increment(ref counter) % 10;
                var likeIds = await _likesRepository.GetCurrentMemberLikeIds(_memberIds[index]);
                likeIds.Should().NotBeNull();
            },
            iterations);

        // Assert
        AssertPerformance(result, maxAverageMs: 100, maxP95Ms: 200);
    }

    [Fact]
    public async Task LikesRepository_AddAndDeleteLike_Performance()
    {
        // Arrange
        var iterations = 50;
        var counter = 50;

        // Act
        var result = await MeasurePerformanceAsync(
            "LikesRepository Add and Delete",
            async () =>
            {
                var sourceIndex = Interlocked.Increment(ref counter) % 50;
                var targetIndex = (sourceIndex + 50) % _memberIds.Count;

                var like = new MemberLike
                {
                    SourceMemberId = _memberIds[sourceIndex],
                    TargetMemberId = _memberIds[targetIndex]
                };

                _likesRepository.AddLike(like);
                await _context.SaveChangesAsync();

                _likesRepository.DeleteLike(like);
                await _context.SaveChangesAsync();
            },
            iterations);

        // Assert
        AssertPerformance(result, maxAverageMs: 200, maxP95Ms: 400);
    }

    [Fact]
    public async Task Repository_BulkOperations_Performance()
    {
        // Arrange
        var iterations = 10;

        // Act
        var result = await MeasurePerformanceAsync(
            "Bulk Repository Operations",
            async () =>
            {
                // Get members
                var memberParams = new MemberParams
                {
                    CurrentMemberId = _memberIds[0],
                    PageNumber = 1,
                    PageSize = 20
                };
                var members = await _memberRepository.GetMembersAsync(memberParams);

                // Get messages
                var messageParams = new MessageParams
                {
                    MemberId = _memberIds[0],
                    Container = "Inbox",
                    PageNumber = 1,
                    PageSize = 20
                };
                var messages = await _messageRepository.GetMessagesForMember(messageParams);

                // Get likes
                var likeParams = new LikesParams
                {
                    MemberId = _memberIds[0],
                    Predicate = "liked",
                    PageNumber = 1,
                    PageSize = 20
                };
                var likes = await _likesRepository.GetMemberLikes(likeParams);

                members.Should().NotBeNull();
                messages.Should().NotBeNull();
                likes.Should().NotBeNull();
            },
            iterations);

        // Assert
        AssertPerformance(result, maxAverageMs: 500, maxP95Ms: 1000);
    }

    public override void Dispose()
    {
        _context?.Dispose();
        base.Dispose();
    }
}