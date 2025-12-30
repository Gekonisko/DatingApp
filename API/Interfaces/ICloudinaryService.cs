using CloudinaryDotNet.Actions;

namespace API.Interfaces;

public interface ICloudinaryService
{
    Task<ImageUploadResult> UploadAsync(ImageUploadParams parameters);

    Task<DeletionResult> DestroyAsync(DeletionParams parameters);
}