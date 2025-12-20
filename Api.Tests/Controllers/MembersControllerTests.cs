using API.Controllers;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using API.Tests.Dummies;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;

namespace Api.Tests.Controllers
{
    public class MembersControllerTests
    {
        private MembersController CreateController(Mock<IMemberRepository> memberMock, Mock<IPhotoService> photoMock)
        {
            var controller = new MembersController(memberMock.Object, photoMock.Object);

            var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "test-user-id")
            }, "TestAuth"));

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };

            return controller;
        }

        [Fact]
        public async Task GetMembers_ShouldReturnListOfMembers()
        {
            var memberMock = new Mock<IMemberRepository>();
            var photoMock = new Mock<IPhotoService>();
            var members = new List<Member>
            {
                new Member { Id = "1", DisplayName = "John", Gender = "", City = "", Country = ""},
                new Member {Id = "2", DisplayName = "Anna", Gender = "", City = "", Country = ""}
            };

            memberMock.Setup(r => r.GetMembersAsync())
                .ReturnsAsync(members);

            var controller = CreateController(memberMock, photoMock);

            var result = await controller.GetMembers();
            var okResult = result.Result as OkObjectResult;
            var returned = okResult!.Value as IReadOnlyList<Member>;

            okResult.Should().NotBeNull();
            returned!.Count.Should().Be(2);
            returned[0].DisplayName.Should().Be("John");
        }

        [Fact]
        public async Task GetMember_ShouldReturnMember_WhenExists()
        {
            var memberMock = new Mock<IMemberRepository>();
            var photoMock = new Mock<IPhotoService>();
            var member = new Member { Id = "123", DisplayName = "Alice", Gender = "", City = "", Country = "" };

            memberMock.Setup(r => r.GetMemberByIdAsync("123"))
                .ReturnsAsync(member);

            var controller = CreateController(memberMock, photoMock);

            var result = await controller.GetMember("123");
            var returned = result.Value;

            returned.Should().NotBeNull();
            returned!.Id.Should().Be("123");
            returned.DisplayName.Should().Be("Alice");
        }

        [Fact]
        public async Task GetMember_ShouldReturn404_WhenNotFound()
        {
            var memberMock = new Mock<IMemberRepository>();
            var photoMock = new Mock<IPhotoService>();
            memberMock.Setup(r => r.GetMemberByIdAsync("999"))
                .ReturnsAsync((Member?)null);

            var controller = CreateController(memberMock, photoMock);

            var result = await controller.GetMember("999");

            result.Result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task GetMemberPhotos_ShouldReturnPhotosList()
        {
            var memberMock = new Mock<IMemberRepository>();
            var photoMock = new Mock<IPhotoService>();
            var photos = new List<Photo>
            {
                new Photo { Id = 1, Url = "/test1.jpg" },
                new Photo { Id = 2, Url = "/test2.jpg" }
            };

            memberMock.Setup(r => r.GetPhotosForMemberAsync("123"))
                .ReturnsAsync(photos);

            var controller = CreateController(memberMock, photoMock);

            var result = await controller.GetMemberPhotos("123");
            var okResult = result.Result as OkObjectResult;
            var returned = okResult!.Value as IReadOnlyList<Photo>;

            returned.Should().NotBeNull();
            returned!.Count.Should().Be(2);
        }

        [Fact]
        public async Task UpdateMember_ShouldReturnNoContent_WhenUpdateSucceeds()
        {
            var memberMock = new Mock<IMemberRepository>();
            var photoMock = new Mock<IPhotoService>();
            var memberId = "test-user-id";
            var existingMember = DummyMemberFactory.Create(id: memberId, displayName: "John Doe");

            memberMock.Setup(r => r.GetMemberForUpdate(memberId))
                .ReturnsAsync(existingMember);

            memberMock.Setup(r => r.SaveAllAsync())
                .ReturnsAsync(true);

            var controller = CreateController(memberMock, photoMock);

            var updateDto = new MemberUpdateDto
            {
                DisplayName = "New Name",
                Description = "New Desc",
                City = "New City",
                Country = "New Country"
            };

            // Act
            var result = await controller.UpdateMember(updateDto);

            // Assert
            result.Should().BeOfType<NoContentResult>();

            existingMember.DisplayName.Should().Be("New Name");
            existingMember.Description.Should().Be("New Desc");
            existingMember.City.Should().Be("New City");
            existingMember.Country.Should().Be("New Country");

            existingMember.User.DisplayName.Should().Be("New Name");

            memberMock.Verify(r => r.Update(existingMember), Times.Once);
        }

        [Fact]
        public async Task UpdateMember_ShouldReturnBadRequest_WhenMemberNotFound()
        {
            // Arrange
            var memberMock = new Mock<IMemberRepository>();
            var photoMock = new Mock<IPhotoService>();
            memberMock.Setup(r => r.GetMemberForUpdate("test-user-id"))
                .ReturnsAsync((Member?)null);

            var controller = CreateController(memberMock, photoMock);

            var dto = new MemberUpdateDto();

            // Act
            var result = await controller.UpdateMember(dto);

            // Assert
            var bad = result as BadRequestObjectResult;

            bad.Should().NotBeNull();
            bad!.Value.Should().Be("Could not get member");
        }

        [Fact]
        public async Task UpdateMember_ShouldReturnBadRequest_WhenSaveFails()
        {
            // Arrange
            var memberMock = new Mock<IMemberRepository>();
            var photoMock = new Mock<IPhotoService>();
            var memberId = "test-user-id";
            var member = DummyMemberFactory.Create(id: memberId, displayName: "John Doe");

            memberMock.Setup(r => r.GetMemberForUpdate(memberId))
                .ReturnsAsync(member);

            memberMock.Setup(r => r.SaveAllAsync())
                .ReturnsAsync(false); // simulate DB failure

            var controller = CreateController(memberMock, photoMock);

            var dto = new MemberUpdateDto { DisplayName = "New" };

            // Act
            var result = await controller.UpdateMember(dto);

            // Assert
            var bad = result as BadRequestObjectResult;

            bad.Should().NotBeNull();
            bad!.Value.Should().Be("Failed to update member");
        }

        [Fact]
        public async Task UpdateMember_ShouldOnlyOverrideProvidedFields()
        {
            // Arrange
            var memberMock = new Mock<IMemberRepository>();
            var photoMock = new Mock<IPhotoService>();
            var memberId = "test-user-id";
            var member = DummyMemberFactory.Create(id: memberId, displayName: "John Doe");

            memberMock.Setup(r => r.GetMemberForUpdate(memberId))
                .ReturnsAsync(member);

            memberMock.Setup(r => r.SaveAllAsync())
                .ReturnsAsync(true);

            var controller = CreateController(memberMock, photoMock);

            var dto = new MemberUpdateDto
            {
                DisplayName = null,   // should NOT change
                City = "New City"     // should change
            };

            // Act
            var result = await controller.UpdateMember(dto);

            // Assert
            result.Should().BeOfType<NoContentResult>();

            member.DisplayName.Should().Be("Name1");  // unchanged
            member.City.Should().Be("New City");      // changed
            member.Description.Should().Be("Desc1");  // unchanged
            member.Country.Should().Be("Country1");   // unchanged
        }
    }
}