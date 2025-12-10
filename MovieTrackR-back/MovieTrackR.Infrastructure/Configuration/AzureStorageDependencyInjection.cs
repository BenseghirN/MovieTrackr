using Azure.Storage.Blobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MovieTrackR.Application.AzureStorage;
using MovieTrackR.Application.AzureStorage.Interfaces;
using MovieTrackR.Infrastructure.AzureStorage.Services;

namespace MovieTrackR.Infrastructure.Configuration;

public static class AzureStorageDependencyInjection
{
    public static IServiceCollection AddAzureStorage(this IServiceCollection services, IConfiguration config)
    {
        services.Configure<AzureStorageOptions>(config.GetSection(AzureStorageOptions.SectionName));

        services.AddSingleton(sp =>
        {
            AzureStorageOptions opt = sp.GetRequiredService<IOptions<AzureStorageOptions>>().Value;

            if (string.IsNullOrWhiteSpace(opt.ConnectionString))
                throw new InvalidOperationException("Azure Blob Storage connection string is not configured.");

            return new BlobServiceClient(opt.ConnectionString);
        });

        services.AddScoped<IAvatarStorageService, AvatarStorageService>();

        return services;
    }
}