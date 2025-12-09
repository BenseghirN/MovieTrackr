namespace MovieTrackR.Application.AzureStorage.Interfaces;

public interface IAvatarStorageService
{
    Task<string> UploadAvatarAsync(Guid userId, byte[] content, string contentType, CancellationToken cancellationToken = default);
}
