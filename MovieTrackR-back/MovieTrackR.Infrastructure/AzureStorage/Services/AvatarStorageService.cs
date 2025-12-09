using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Options;
using MovieTrackR.Application.AzureStorage;
using MovieTrackR.Application.AzureStorage.Interfaces;

namespace MovieTrackR.Infrastructure.AzureStorage.Services;

internal sealed class AvatarStorageService(BlobServiceClient blobServiceClient, IOptionsSnapshot<AzureStorageOptions> options) : IAvatarStorageService
{
    private AzureStorageOptions AzureStorageOptions => options.Value;

    public async Task<string> UploadAvatarAsync(Guid userId, byte[] content, string contentType, CancellationToken cancellationToken = default)
    {
        BlobContainerClient container = await GetContainerAsync(cancellationToken);
        string blobName = $"{userId}/avatar-{Guid.NewGuid():N}";
        BlobClient blob = container.GetBlobClient(blobName);

        using MemoryStream ms = new MemoryStream(content);
        await blob.UploadAsync(ms, new BlobUploadOptions
        {
            HttpHeaders = new BlobHttpHeaders { ContentType = contentType }
        }, cancellationToken);

        return blob.Uri.ToString();
    }

    private async Task<BlobContainerClient> GetContainerAsync(CancellationToken cancellationToken)
    {
        BlobContainerClient container = blobServiceClient.GetBlobContainerClient(AzureStorageOptions.AvatarsContainerName);

        await container.CreateIfNotExistsAsync(cancellationToken: cancellationToken);

        return container;
    }
}