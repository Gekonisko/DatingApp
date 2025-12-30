using API.Helpers;
using API.Interfaces;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.Extensions.Options;

namespace API.Services;

public class CloudinaryService : ICloudinaryService
{
    private readonly Cloudinary _cloudinary;

    public CloudinaryService(IOptions<CloudinarySettings> config)
    {
        var account = new Account(
            config.Value.CloudName,
            config.Value.ApiKey,
            config.Value.ApiSecret);

        _cloudinary = new Cloudinary(account);
    }

    public Task<ImageUploadResult> UploadAsync(ImageUploadParams parameters)
        => _cloudinary.UploadAsync(parameters);

    public Task<DeletionResult> DestroyAsync(DeletionParams parameters)
        => _cloudinary.DestroyAsync(parameters);
}