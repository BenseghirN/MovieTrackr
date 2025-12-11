using MediatR;
using Microsoft.EntityFrameworkCore;
using MovieTrackR.Application.Common.Exceptions;
using MovieTrackR.Application.DTOs;
using MovieTrackR.Application.Interfaces;
using MovieTrackR.Domain.Entities;

namespace MovieTrackR.Application.People.Commands;

public sealed record UpdatePersonCommand(Guid Id, UpdatePersonDto Dto) : IRequest;

public sealed class UpdateMovieHandler(IMovieTrackRDbContext dbContext)
    : IRequestHandler<UpdatePersonCommand>
{
    public async Task Handle(UpdatePersonCommand updateCommand, CancellationToken cancellationToken)
    {
        Person person = await dbContext.People
            .FirstOrDefaultAsync(x => x.Id == updateCommand.Id, cancellationToken)
            ?? throw new NotFoundException("Person not found.");

        bool exists = await dbContext.People.AnyAsync(
            x => x.Id != updateCommand.Id && x.Name == updateCommand.Dto.Name && x.BirthDate == updateCommand.Dto.BirthDate, cancellationToken);
        if (exists) throw new ConflictException("Another movie with same title & year exists.");

        person.UpdateDetails(
            name: updateCommand.Dto.Name,
            birth: updateCommand.Dto.BirthDate,
            death: updateCommand.Dto.DeathDate,
            bio: updateCommand.Dto.Biography,
            placeOfBirth: updateCommand.Dto.PlaceOfBirth,
            profilePath: updateCommand.Dto.ProfilePath,
            knownForDepartment: null
        );

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}