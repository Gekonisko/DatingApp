using API.Data;
using API.Entities;
using API.Helpers;
using FluentAssertions;

namespace Api.Tests.IntegrationTests.Repositories;

public class LikesRepositoryTests : IntegrationTestBase
{
    private LikesRepository CreateRepository() => CreateLikesRepository();

    private async Task SeedMembersAsync()
    {
        var members = new[]
        {
            new Member
            {
                Id = "member-1",
                DisplayName = "User 1",
                DateOfBirth = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-25)),
                Gender = "male",
                City = "City",
                Country = "Country"
            },
            new Member
            {
                Id = "member-2",
                DisplayName = "User 2",
                DateOfBirth = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-30)),
                Gender = "male",
                City = "City",
                Country = "Country"
            },
            new Member
            {
                Id = "member-3",
                DisplayName = "User 3",
                DateOfBirth = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-35)),
                Gender = "male",
                City = "City",
                Country = "Country"
            }
        };

        Context.Members.AddRange(members);
        await Context.SaveChangesAsync();
    }

    private async Task SeedLikesAsync()
    {
        Context.Likes.AddRange(
            new MemberLike
            {
                SourceMemberId = "member-1",
                TargetMemberId = "member-2"
            },
            new MemberLike
            {
                SourceMemberId = "member-2",
                TargetMemberId = "member-1"
            },
            new MemberLike
            {
                SourceMemberId = "member-1",
                TargetMemberId = "member-3"
            }
        );

        await Context.SaveChangesAsync();
    }

    [Fact]
    public async Task AddLike_Should_Persist_Like()
    {
        await SeedMembersAsync();
        var repo = CreateRepository();

        var like = new MemberLike
        {
            SourceMemberId = "member-1",
            TargetMemberId = "member-2"
        };

        repo.AddLike(like);
        await Context.SaveChangesAsync();

        var result = await repo.GetMemberLike("member-1", "member-2");

        result.Should().NotBeNull();
        result!.SourceMemberId.Should().Be("member-1");
        result.TargetMemberId.Should().Be("member-2");
    }

    [Fact]
    public async Task DeleteLike_Should_Remove_Like()
    {
        await SeedMembersAsync();
        await SeedLikesAsync();

        var repo = CreateRepository();
        var like = await repo.GetMemberLike("member-1", "member-2");
        like.Should().NotBeNull();

        repo.DeleteLike(like!);
        await Context.SaveChangesAsync();

        var deleted = await repo.GetMemberLike("member-1", "member-2");
        deleted.Should().BeNull();
    }

    [Fact]
    public async Task GetCurrentMemberLikeIds_Should_Return_All_Target_Ids()
    {
        await SeedMembersAsync();
        await SeedLikesAsync();

        var repo = CreateRepository();
        var result = await repo.GetCurrentMemberLikeIds("member-1");

        result.Should().BeEquivalentTo(new[] { "member-2", "member-3" });
    }

    [Fact]
    public async Task GetMemberLikes_Liked_Should_Return_Liked_Members()
    {
        await SeedMembersAsync();
        await SeedLikesAsync();

        var repo = CreateRepository();

        var parameters = new LikesParams
        {
            MemberId = "member-1",
            Predicate = "liked",
            PageNumber = 1,
            PageSize = 10
        };

        var result = await repo.GetMemberLikes(parameters);

        result.Items.Select(x => x.Id)
            .Should().BeEquivalentTo(new[] { "member-2", "member-3" });
    }

    [Fact]
    public async Task GetMemberLikes_LikedBy_Should_Return_Members_Who_Liked_Me()
    {
        await SeedMembersAsync();
        await SeedLikesAsync();

        var repo = CreateRepository();

        var parameters = new LikesParams
        {
            MemberId = "member-1",
            Predicate = "likedBy",
            PageNumber = 1,
            PageSize = 10
        };

        var result = await repo.GetMemberLikes(parameters);

        result.Items.Select(x => x.Id)
            .Should().BeEquivalentTo(new[] { "member-2" });
    }

    [Fact]
    public async Task GetMemberLikes_Mutual_Should_Return_Mutual_Likes()
    {
        await SeedMembersAsync();
        await SeedLikesAsync();

        var repo = CreateRepository();

        var parameters = new LikesParams
        {
            MemberId = "member-1",
            Predicate = "",
            PageNumber = 1,
            PageSize = 10
        };

        var result = await repo.GetMemberLikes(parameters);

        result.Items.Select(x => x.Id)
            .Should().BeEquivalentTo(new[] { "member-2" });
    }

    [Fact]
    public async Task GetMemberLikes_Should_Respect_Pagination()
    {
        await SeedMembersAsync();

        Context.Likes.AddRange(
            Enumerable.Range(1, 20).Select(i =>
                new MemberLike
                {
                    SourceMemberId = "member-1",
                    TargetMemberId = "member-2"
                }));

        await Context.SaveChangesAsync();

        var repo = CreateRepository();

        var parameters = new LikesParams
        {
            MemberId = "member-1",
            Predicate = "liked",
            PageNumber = 1,
            PageSize = 5
        };

        var result = await repo.GetMemberLikes(parameters);

        result.Items.Should().HaveCount(5);
        result.Items.Count.Should().Be(20);
    }
}