using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MovieTrackR.Application.Common.Commands;
using MovieTrackR.Application.DTOs;
using MovieTrackR.Application.Interfaces;

namespace MovieTrackR.Application.UserLists.Queries;

public sealed record GetListByIdQuery(CurrentUserDto currentUser, Guid ListId) : IRequest<UserListDetailsDto?>;
public sealed class GetListByIdHandler(IMovieTrackRDbContext dbContext, IMapper mapper, ISender sender)
    : IRequestHandler<GetListByIdQuery, UserListDetailsDto?>
{
    public async Task<UserListDetailsDto?> Handle(GetListByIdQuery query, CancellationToken cancellationToken)
    {
        Guid userId = await sender.Send(new EnsureUserExistsCommand(query.currentUser), cancellationToken);
        // Si ProjectTo ne traduit pas bien le OrderBy imbriqué, projection manuelle

        // return await dbContext.UserLists
        //     .Where(l => l.Id == query.ListId && l.UserId == userId)
        //     .Select(l => new UserListDetailsDto {
        //         Id = l.Id,
        //         Title = l.Title,
        //         Description = l.Description,
        //         CreatedAt = l.CreatedAt,
        //         Movies = l.Movies
        //             .OrderBy(m => m.Position)
        //             .Select(m => new UserListMovieDto {
        //                 MovieId = m.MovieId,
        //                 Position = m.Position,
        //                 Movie = new MovieSummaryDto {
        //                     Id = m.Movie.Id,
        //                     Title = m.Movie.Title,
        //                     Year = m.Movie.Year,
        //                     PosterUrl = m.Movie.PosterUrl
        //                 }
        //             }).ToList()
        //     })
        //     .FirstOrDefaultAsync(cancellationToken);

        return await dbContext.UserLists
            .Where(l => l.Id == query.ListId && l.UserId == userId)
            .ProjectTo<UserListDetailsDto>(mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(cancellationToken);
    }
}