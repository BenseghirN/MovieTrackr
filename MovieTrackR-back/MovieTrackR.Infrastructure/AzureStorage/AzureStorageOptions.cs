namespace MovieTrackR.Application.AzureStorage;

public sealed class AzureStorageOptions
{
    public const string SectionName = "AzureStorage";
    public string ConnectionString { get; set; } = string.Empty;
    public string AvatarsContainerName { get; set; } = "avatars";
}