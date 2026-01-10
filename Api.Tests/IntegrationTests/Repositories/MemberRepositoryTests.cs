using API.Entities;
using API.Helpers;
using FluentAssertions;

namespace Api.Tests.IntegrationTests.Repositories;

public class MemberRepositoryTests : IntegrationTestBase
{
    [Fact]
    public async Task GetMemberByIdAsync_Should_Return_Member()
    {
        var member = new Member
        {
            Id = "member-1",
            DisplayName = "Test",
            Gender = "male",
            DateOfBirth = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-25)),
            City = "TestCity",
            Country = "TestCountry"
        };

        await SeedMembersAsync(member);

        var repo = CreateMemberRepository();
        var result = await repo.GetMemberByIdAsync("member-1");

        result.Should().NotBeNull();
        result!.Id.Should().Be("member-1");
    }

    [Fact]
    public async Task GetMemberForUpdate_Should_Include_User_And_Photos()
    {
        var member = new Member
        {
            Id = "member-2",
            DisplayName = "User",
            Gender = "female",
            DateOfBirth = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-30)),
            City = "TestCity",
            Country = "TestCountry",
            User = new AppUser { Id = "member-2", UserName = "user@test.com", DisplayName = "test user" },
            Photos = new List<Photo>
            {
                new Photo { Url = "photo.jpg" }
            }
        };

        await SeedMembersAsync(member);

        var repo = CreateMemberRepository();
        var result = await repo.GetMemberForUpdate("member-2");

        result!.User.Should().NotBeNull();
        result.Photos.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetMembersAsync_Should_Filter_By_Gender_And_Age()
    {
        var members = new[]
        {
        new Member
        {
            Id = "1",
            Gender = "male",
            DateOfBirth = DateOnly.FromDateTime(DateTime.Today.AddYears(-25)),
            Created = DateTime.UtcNow.AddDays(-10),
            LastActive = DateTime.UtcNow,
            City = "TestCity",
            Country = "TestCountry",
            DisplayName = "Name"
        },
        new Member
        {
            Id = "2",
            Gender = "female",
            DateOfBirth = DateOnly.FromDateTime(DateTime.Today.AddYears(-35)),
            Created = DateTime.UtcNow.AddDays(-5),
            LastActive = DateTime.UtcNow,
            City = "TestCity",
            Country = "TestCountry",
            DisplayName = "Name",
        }
    };

        await SeedMembersAsync(members);

        var repo = CreateMemberRepository();

        var parameters = new MemberParams
        {
            CurrentMemberId = "1",
            Gender = "female",
            MinAge = 30,
            MaxAge = 40,
            PageNumber = 1,
            PageSize = 10
        };

        var result = await repo.GetMembersAsync(parameters);

        result.Items.Should().ContainSingle();
        result.Items.First().Id.Should().Be("2");
    }

    [Fact]
    public async Task GetPhotosForMemberAsync_Should_Respect_Query_Filters()
    {
        var member = new Member
        {
            Id = "member-3",
            Gender = "male",
            City = "TestCity",
            Country = "TestCountry",
            DisplayName = "Name",
            Photos = new List<Photo>
        {
            new Photo { Url = "1.jpg", IsApproved = true },
            new Photo { Url = "2.jpg", IsApproved = false }
        }
        };

        await SeedMembersAsync(member);

        var repo = CreateMemberRepository();

        var publicPhotos = await repo.GetPhotosForMemberAsync("member-3", false);
        publicPhotos.Should().HaveCount(1);

        var allPhotos = await repo.GetPhotosForMemberAsync("member-3", true);
        allPhotos.Should().HaveCount(2);
    }

    protected async Task SeedMembersAsync(params Member[] members)
    {
        Context.Members.AddRange(members);

        // Ensure corresponding AppUser rows exist for each Member (FK Members.Id -> AspNetUsers.Id)
        foreach (var m in members)
        {
            // If a User navigation was provided, ensure its Id matches the Member.Id
            if (m.User != null)
            {
                m.User.Id = m.Id;
                // Add the same AppUser instance so EF tracks the relationship correctly
                var exists = await Context.Users.FindAsync(m.Id);
                if (exists == null)
                {
                    Context.Users.Add(m.User);
                }
            }
            else
            {
                var exists = await Context.Users.FindAsync(m.Id);
                if (exists == null)
                {
                    var appUser = new AppUser
                    {
                        Id = m.Id,
                        UserName = m.Id + "@test.local",
                        NormalizedUserName = (m.Id + "@test.local").ToUpperInvariant(),
                        Email = m.Id + "@test.local",
                        NormalizedEmail = (m.Id + "@test.local").ToUpperInvariant(),
                        DisplayName = m.DisplayName
                    };
                    Context.Users.Add(appUser);
                }
            }
        }

        await Context.SaveChangesAsync();
    }
}