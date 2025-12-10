using MediatR;
using Microsoft.EntityFrameworkCore;
using MovieTrackR.Application.Common.Exceptions;
using MovieTrackR.Application.DTOs;
using MovieTrackR.Application.Interfaces;
using MovieTrackR.Domain.Entities;

namespace MovieTrackR.Application.People.Commands;

public sealed record CreatePersonCommand(CreatePersonDto Dto) : IRequest<Guid>;

public sealed class CreateMovieHandler(IMovieTrackRDbContext dbContext)
    : IRequestHandler<CreatePersonCommand, Guid>
{
    public async Task<Guid> Handle(CreatePersonCommand createCommand, CancellationToken cancellationToken)
    {
        bool exists = await dbContext.People.AnyAsync(
            p => p.Name == createCommand.Dto.Name && p.BirthDate == createCommand.Dto.BirthDate, cancellationToken);
        if (exists) throw new ConflictException("Movie already exists (title+year).");

        Person person = Person.Create(
            name: createCommand.Dto.Name,
            birthDate: createCommand.Dto.BirthDate,
            deathDate: createCommand.Dto.DeathDate,
            biography: createCommand.Dto.Biography,
            placeOfBirth: createCommand.Dto.PlaceOfBirth,
            tmdbId: createCommand.Dto.TmdbId,
            ProfilePictureUrl: createCommand.Dto.ProfilePath
        );

        dbContext.People.Add(person);
        await dbContext.SaveChangesAsync(cancellationToken);
        return person.Id;
    }
}