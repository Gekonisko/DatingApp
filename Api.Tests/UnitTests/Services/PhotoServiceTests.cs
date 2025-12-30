using API.Interfaces;
using API.Services;
using CloudinaryDotNet.Actions;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;

namespace Api.Tests.UnitTests.Services;

public class PhotoServiceTests
{
    private readonly Mock<ICloudinaryService> _cloudinaryMock;
    private readonly PhotoService _photoService;

    public PhotoServiceTests()
    {
        _cloudinaryMock = new Mock<ICloudinaryService>();
        _photoService = new PhotoService(_cloudinaryMock.Object);
    }

    [Fact]
    public async Task UploadPhotoAsync_Should_Upload_When_File_Has_Content()
    {
        // Arrange
        var fileMock = new Mock<IFormFile>();
        var content = "fake image content";
        var fileName = "photo.jpg";
        var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(content));

        fileMock.Setup(f => f.Length).Returns(stream.Length);
        fileMock.Setup(f => f.FileName).Returns(fileName);
        fileMock.Setup(f => f.OpenReadStream()).Returns(stream);

        var uploadResult = new ImageUploadResult
        {
            SecureUrl = new Uri("http://cloudinary/image.jpg"),
            PublicId = "public-id"
        };

        _cloudinaryMock
            .Setup(x => x.UploadAsync(It.IsAny<ImageUploadParams>()))
            .ReturnsAsync(uploadResult);

        // Act
        var result = await _photoService.UploadPhotoAsync(fileMock.Object);

        // Assert
        result.Should().NotBeNull();
        result.PublicId.Should().Be("public-id");

        _cloudinaryMock.Verify(
            x => x.UploadAsync(It.IsAny<ImageUploadParams>()),
            Times.Once);
    }

    [Fact]
    public async Task UploadPhotoAsync_Should_Return_Empty_Result_When_File_Is_Empty()
    {
        // Arrange
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.Length).Returns(0);

        // Act
        var result = await _photoService.UploadPhotoAsync(fileMock.Object);

        // Assert
        result.Should().NotBeNull();

        _cloudinaryMock.Verify(
            x => x.UploadAsync(It.IsAny<ImageUploadParams>()),
            Times.Never);
    }

    [Fact]
    public async Task DeletePhotoAsync_Should_Call_Destroy()
    {
        // Arrange
        var deletionResult = new DeletionResult { Result = "ok" };

        _cloudinaryMock
            .Setup(x => x.DestroyAsync(It.IsAny<DeletionParams>()))
            .ReturnsAsync(deletionResult);

        // Act
        var result = await _photoService.DeletePhotoAsync("public-id");

        // Assert
        result.Result.Should().Be("ok");

        _cloudinaryMock.Verify(
            x => x.DestroyAsync(It.IsAny<DeletionParams>()),
            Times.Once);
    }
}