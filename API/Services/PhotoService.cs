using API.Interfaces;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;

namespace API.Services;

public class PhotoService : IPhotoService
{
    private readonly ICloudinaryService _cloudinary;

    public PhotoService(ICloudinaryService cloudinary)
    {
        _cloudinary = cloudinary;
    }

    public async Task<DeletionResult> DeletePhotoAsync(string publicId)
    {
        var deleteParams = new DeletionParams(publicId);
        return await _cloudinary.DestroyAsync(deleteParams);
    }

    public async Task<ImageUploadResult> UploadPhotoAsync(IFormFile file)
    {
        if (file.Length == 0) return new ImageUploadResult();

        await using var stream = file.OpenReadStream();

        var uploadParams = new ImageUploadParams
        {
            File = new FileDescription(file.FileName, stream),
            Transformation = new Transformation()
                .Height(500).Width(500).Crop("fill").Gravity("face"),
            Folder = "da-ang20"
        };

        return await _cloudinary.UploadAsync(uploadParams);
    }
}