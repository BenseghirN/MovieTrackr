using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MovieTrackR.Application.DTOs;
using MovieTrackR.Application.Interfaces;
using MovieTrackR.Application.TMDB;
using MovieTrackR.Application.TMDB.Interfaces;
using MovieTrackR.Domain.Entities;
using MovieTrackR.Domain.Helpers;

namespace MovieTrackR.Application.People.Queries;

public sealed record GetPersonMovieCreditsQuery(Guid PersonId) : IRequest<List<PersonMovieCreditDto>?>;

public sealed class GetPersonMovieCreditsHandler(IMovieTrackRDbContext dbContext, ITmdbClient tmdbClient, IMapper mapper)
    : IRequestHandler<GetPersonMovieCreditsQuery, List<PersonMovieCreditDto>?>
{
    public async Task<List<PersonMovieCreditDto>?> Handle(GetPersonMovieCreditsQuery query, CancellationToken cancellationToken)
    {
        Person? person = await dbContext.People
            .AsNoTracking()
            .Include(p => p.CastRoles).ThenInclude(c => c.Movie)
            .Include(p => p.CrewRoles).ThenInclude(c => c.Movie)
            .FirstOrDefaultAsync(p => p.Id == query.PersonId, cancellationToken);

        if (person == null) return null;

        // 1. Crédits locaux
        List<PersonMovieCreditDto> localCredits = GetLocalCredits(person);

        if (localCredits.Count() < 10 && person.TmdbId.HasValue)
        {
            List<PersonMovieCreditDto> tmdbCredits = await GetTmdbCreditsAsync(person.TmdbId.Value, cancellationToken);

            // Fusionner sans doublons
            HashSet<int> localTmdbIds = localCredits
                .Where(c => c.TmdbMovieId.HasValue)
                .Select(c => c.TmdbMovieId!.Value)
                .ToHashSet();

            List<PersonMovieCreditDto> uniqueTmdbCredits = tmdbCredits
                .Where(c => c.TmdbMovieId.HasValue && !localTmdbIds.Contains(c.TmdbMovieId.Value))
                .ToList();

            return localCredits
                .Concat(uniqueTmdbCredits)
                .OrderByDescending(c => c.Year ?? 0)
                .Take(20)
                .ToList();
        }

        return localCredits;
    }

    private List<PersonMovieCreditDto> GetLocalCredits(Person person)
    {
        IEnumerable<PersonMovieCreditDto> castCredits = person.CastRoles
            .AsQueryable()
            .ProjectTo<PersonMovieCreditDto>(mapper.ConfigurationProvider)
            .ToList();

        IEnumerable<PersonMovieCreditDto> crewCredits = person.CrewRoles
            .Where(c => CrewHelpers.IsImportantJob(c.Job))
            .AsQueryable()
            .ProjectTo<PersonMovieCreditDto>(mapper.ConfigurationProvider)
            .ToList();

        return castCredits.Concat(crewCredits)
            .OrderByDescending(c => c.Year ?? 0)
            .ToList();
    }

    private async Task<List<PersonMovieCreditDto>> GetTmdbCreditsAsync(int tmdbId, CancellationToken cancellationToken)
    {
        try
        {
            TmdbPersonMovieCredits? tmdbCredits = await tmdbClient.GetPersonMovieCreditsAsync(tmdbId, "fr-FR", cancellationToken);
            if (tmdbCredits == null) return [];

            List<PersonMovieCreditDto> castCredits = mapper.Map<List<PersonMovieCreditDto>>(tmdbCredits.Cast);

            List<TmdbPersonCrewCredit> importantCrew = tmdbCredits.Crew
            .Where(c => !string.IsNullOrWhiteSpace(c.Job) && CrewHelpers.IsImportantJob(c.Job))
            .ToList();

            List<PersonMovieCreditDto> crewCredits = mapper.Map<List<PersonMovieCreditDto>>(importantCrew);

            return castCredits.Concat(crewCredits)
                .OrderByDescending(c => c.Year ?? 0)
                .ToList();
        }
        catch
        {
            return [];
        }
    }
}