using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MovieTrackR.Application.DTOs;
using MovieTrackR.Application.Interfaces;
using MovieTrackR.Domain.Entities;

namespace MovieTrackR.Application.Movies.Queries;

public sealed record GetTrendingMoviesQuery()
    : IRequest<IReadOnlyList<SearchMovieResultDto>>;

public sealed class GetTrendingMoviesHandler(IMovieTrackRDbContext dbContext, IMapper mapper)
    : IRequestHandler<GetTrendingMoviesQuery, IReadOnlyList<SearchMovieResultDto>>
{
    public async Task<IReadOnlyList<SearchMovieResultDto>> Handle(GetTrendingMoviesQuery searchQuery, CancellationToken cancellationToken)
    {
        DateTime since = DateTime.UtcNow.AddDays(-30);

        var items =
            await (
                from m in dbContext.Movies

                let recentReviews = m.Reviews
                    .Where(r => r.CreatedAt >= since)

                let recentComments = m.Reviews
                    .SelectMany(r => r.Comments)
                    .Where(c => c.CreatedAt >= since)

                let recentLikes = m.Reviews
                    .SelectMany(r => r.Likes)
                    .Where(l => l.CreatedAt >= since)

                let reviewCount = recentReviews.Count()
                let commentCount = recentComments.Count()
                let likeCount = recentLikes.Count()

                let trendingScore =
                      reviewCount * 3
                    + commentCount * 1
                    + likeCount * 2

                where trendingScore > 0

                select new
                {
                    Movie = m,
                    TrendingScore = trendingScore,
                    LastActivity = new[]
                    {
                        recentReviews.Max(r => (DateTime?)r.CreatedAt),
                        recentComments.Max(c => (DateTime?)c.CreatedAt),
                        recentLikes.Max(l => (DateTime?)l.CreatedAt)
                    }.Max()
                }
            )
            .OrderByDescending(x => x.TrendingScore)
            .ThenByDescending(x => x.LastActivity)
            .Take(20)
            .ToListAsync(cancellationToken);

        List<Movie> movies = items.Select(x => x.Movie).ToList();
        return mapper.Map<IReadOnlyList<SearchMovieResultDto>>(movies);
    }
}