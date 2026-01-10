using API.Data;
using API.Entities;
using FluentAssertions;

namespace Api.Tests.IntegrationTests.Repositories;

public class PhotoRepositoryTests : IntegrationTestBase
{
    private PhotoRepository CreateRepository() => new(Context);

    private async Task SeedMemberWithPhotosAsync()
    {
        var member = new Member
        {
            Id = "member-1",
            DisplayName = "User",
            Gender = "male",
            City = "City",
            Country = "Country",
            Photos =
            {
                new Photo
                {
                    Id = 1,
                    Url = "approved.jpg",
                    IsApproved = true,
                    MemberId = "member-1"
                },
                new Photo
                {
                    Id = 2,
                    Url = "unapproved.jpg",
                    IsApproved = false,
                    MemberId = "member-1"
                }
            }
        };

        Context.Members.Add(member);
        // Ensure corresponding AppUser exists for FK Members(Id) -> AspNetUsers(Id)
        var exists = await Context.Users.FindAsync(member.Id);
        if (exists == null)
        {
            Context.Users.Add(new API.Entities.AppUser
            {
                Id = member.Id,
                UserName = member.Id + "@test.local",
                NormalizedUserName = (member.Id + "@test.local").ToUpperInvariant(),
                Email = member.Id + "@test.local",
                NormalizedEmail = (member.Id + "@test.local").ToUpperInvariant(),
                DisplayName = member.DisplayName
            });
        }

        await Context.SaveChangesAsync();
    }

    [Fact]
    public async Task GetPhotoById_Should_Return_Photo_Ignoring_Query_Filters()
    {
        await SeedMemberWithPhotosAsync();
        var repo = CreateRepository();

        var photo = await repo.GetPhotoById(2);

        photo.Should().NotBeNull();
        photo!.IsApproved.Should().BeFalse();
        photo.Url.Should().Be("unapproved.jpg");
    }

    [Fact]
    public async Task GetUnapprovedPhotos_Should_Return_Only_Unapproved_Photos()
    {
        await SeedMemberWithPhotosAsync();
        var repo = CreateRepository();

        var result = await repo.GetUnapprovedPhotos();

        result.Should().HaveCount(1);
        result.First().Url.Should().Be("unapproved.jpg");
        result.First().IsApproved.Should().BeFalse();
    }

    [Fact]
    public async Task RemovePhoto_Should_Delete_Photo()
    {
        await SeedMemberWithPhotosAsync();
        var repo = CreateRepository();

        var photo = await repo.GetPhotoById(1);
        photo.Should().NotBeNull();

        repo.RemovePhoto(photo!);
        await Context.SaveChangesAsync();

        var deleted = await repo.GetPhotoById(1);
        deleted.Should().BeNull();
    }
}