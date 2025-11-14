using FluentValidation;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MovieTrackR.Application.behaviors;
using MovieTrackR.Application.Mapping;

namespace MovieTrackR.Application.Configuration;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
    {
        // AutoMapper : Add all profiles from Application
        services.AddAutoMapper(cfg => { }, typeof(MappingProfile).Assembly);
        services.AddHttpContextAccessor();

        // MediatR : Registering all Requests/Handlers from Application
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));

        // FluentValidation : Registering all validators from Application
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        // Return validation errors as ValidationException 
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        return services;
    }
}
