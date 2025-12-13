using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using MovieTrackR.AI.Utils;

namespace MovieTrackR.AI.Builder;

public sealed class SemanticKernelBuilder(IOptions<AiOptions> options)
{
    private readonly AiOptions _options = options.Value;
    public Kernel BuildKernel(string? serviceId = null)
    {
        IKernelBuilder builder = Kernel.CreateBuilder();

        builder.AddAzureOpenAIChatCompletion(
            deploymentName: _options.ModelName,
            apiKey: _options.ApiKey,
            endpoint: _options.EndpointUrl,
            serviceId: serviceId
        );

        return builder.Build();
    }
}