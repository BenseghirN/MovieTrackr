using AutoMapper;
using MovieTrackR.Application.Common.Helpers;
using MovieTrackR.Application.DTOs;
using MovieTrackR.Application.TMDB;
using MovieTrackR.Domain.Entities;
using MovieTrackR.Domain.Enums;
using MovieTrackR.Domain.Helpers;

namespace MovieTrackR.Application.Mapping;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<User, UserDto>();
        CreateMap<Movie, MovieDetailsDto>()
            .ForMember(d => d.Genres, opt => opt.MapFrom(s => s.MovieGenres.Select(mg => mg.Genre)))
            .ForMember(d => d.Cast, opt => opt.MapFrom(s => s.Cast.OrderBy(c => c.Order).Take(20)))
            .ForMember(d => d.Crew, opt => opt.MapFrom(s =>
                s.Crew
                    .Where(c => CrewHelpers.IsImportantJob(c.Job))
                    .OrderBy(c => CrewHelpers.GetJobPriority(c.Job))
                    .Take(10)));

        CreateMap<MovieCast, CastMemberDto>()
            .ForMember(d => d.PersonId, opt => opt.MapFrom(s => s.Person.Id))
            .ForMember(d => d.Name, opt => opt.MapFrom(s => s.Person.Name))
            .ForMember(d => d.Character, opt => opt.MapFrom(s => s.CharacterName))
            .ForMember(d => d.ProfilePath, opt => opt.MapFrom(s => s.Person.ProfilePictureUrl));

        CreateMap<MovieCrew, CrewMemberDto>()
            .ForMember(d => d.PersonId, opt => opt.MapFrom(s => s.Person.Id))
            .ForMember(d => d.Name, opt => opt.MapFrom(s => s.Person.Name))
            .ForMember(d => d.ProfilePath, opt => opt.MapFrom(s => s.Person.ProfilePictureUrl));

        CreateMap<UserList, UserListDto>()
            .ForMember(d => d.MoviesCount, opt => opt.MapFrom(s => s.Movies.Count))
            .ForMember(d => d.Type, opt => opt.MapFrom(s => s.Type))
            .ForMember(d => d.IsSystemList, opt => opt.MapFrom(s => s.IsSystemList));

        CreateMap<UserList, UserListDetailsDto>()
            .ForMember(d => d.Movies, opt => opt.MapFrom(
                l => l.Movies.OrderBy(m => m.Position)))
            .ForMember(d => d.Type, opt => opt.MapFrom(l => l.Type))
            .ForMember(d => d.IsSystemList, opt => opt.MapFrom(s => s.IsSystemList));

        CreateMap<UserListMovie, UserListMovieDto>()
            .ForMember(d => d.MovieId, opt => opt.MapFrom(s => s.MovieId))
            .ForMember(d => d.Position, opt => opt.MapFrom(s => s.Position))
            .ForMember(d => d.Movie, opt => opt.MapFrom(s => s.Movie));

        CreateMap<Movie, MovieSummaryDto>()
            .ForMember(d => d.Id, opt => opt.MapFrom(s => s.Id))
            .ForMember(d => d.Title, opt => opt.MapFrom(s => s.Title))
            .ForMember(d => d.Year, opt => opt.MapFrom(s => s.Year))
            .ForMember(d => d.PosterUrl, opt => opt.MapFrom(s => s.PosterUrl));

        CreateMap<Genre, GenreDto>();

        // Review -> List item
        CreateMap<Review, ReviewListItemDto>()
            .ForMember(d => d.UserId, opt => opt.MapFrom(s => s.UserId))
            .ForMember(d => d.UserName, opt => opt.MapFrom(s => s.User.Pseudo))
            .ForMember(d => d.Rating, opt => opt.MapFrom(s => s.Rating))
            .ForMember(d => d.Content, opt => opt.MapFrom(s => s.Content))
            .ForMember(d => d.CreatedAt, opt => opt.MapFrom(s => s.CreatedAt))
            .ForMember(d => d.LikesCount, opt => opt.MapFrom(s => s.Likes.Count))
            .ForMember(d => d.CommentsCount, opt => opt.MapFrom(s => s.Comments.Count))
            .ForMember(d => d.HasLiked, opt => opt.Ignore())
            .AfterMap((src, dest, context) =>
            {
                if (context.Items.TryGetValue("CurrentUserId", out object? userIdObj) && userIdObj is Guid userId)
                {
                    dest.HasLiked = src.Likes.Any(l => l.UserId == userId);
                }
            });

        // Review -> Details
        CreateMap<Review, ReviewDetailsDto>()
            .ForMember(d => d.LikesCount, opt => opt.MapFrom(s => s.Likes.Count))
            .ForMember(d => d.CommentsCount, opt => opt.MapFrom(s => s.Comments.Count));

        // MovieProposal
        CreateMap<MovieProposal, MovieProposalListItemDto>();
        CreateMap<MovieProposal, MovieProposalDetailsDto>();
        CreateMap<CreateMovieProposalDto, MovieProposal>()
            .ForMember(d => d.Id, opt => opt.Ignore())
            .ForMember(d => d.ProposedByUserId, opt => opt.Ignore())
            .ForMember(d => d.Status, opt => opt.MapFrom(_ => MovieProposalStatus.UnderReview))
            .ForMember(d => d.ModerationNote, opt => opt.Ignore())
            .ForMember(d => d.ReviewedAt, opt => opt.Ignore())
            .ForMember(d => d.ProposedByUser, opt => opt.Ignore());

        CreateMap<ReviewComment, CommentDto>()
            .ForMember(d => d.UserName, opt => opt.MapFrom(s => s.User.Pseudo));

        // MOVIE DB -> DTO
        CreateMap<Movie, SearchMovieResultDto>(MemberList.Source)
            .ForMember(d => d.LocalId, opt => opt.MapFrom(s => s.Id))
            .ForMember(d => d.TmdbId, opt => opt.MapFrom(s => s.TmdbId))
            .ForMember(d => d.Title, opt => opt.MapFrom(s => s.Title))
            .ForMember(d => d.Year, opt => opt.MapFrom(s => s.ReleaseDate.HasValue ? (int?)s.ReleaseDate.Value.Year : null))
            .ForMember(d => d.OriginalTitle, opt => opt.MapFrom(s => s.OriginalTitle))
            .ForMember(d => d.PosterPath, opt => opt.MapFrom(s => s.PosterUrl)) // ajuste si c'est PosterUrl côté entity
            .ForMember(d => d.IsLocal, opt => opt.MapFrom(_ => true))
            .ForMember(d => d.Overview, opt => opt.MapFrom(s => s.Overview));

        // MOVIE TMDb -> DTO
        CreateMap<TmdbSearchMovieItem, SearchMovieResultDto>(MemberList.Source)
            .ForMember(d => d.LocalId, opt => opt.Ignore())
            .ForMember(d => d.TmdbId, opt => opt.MapFrom(s => s.Id))
            .ForMember(d => d.Title, opt => opt.MapFrom(s => s.Title ?? s.OriginalTitle ?? "(Sans titre)"))
            .ForMember(d => d.Year, opt => opt.MapFrom(s => TmdbMapHelpers.YearFromReleaseDate(s.ReleaseDate)))
            .ForMember(d => d.OriginalTitle, opt => opt.MapFrom(s => s.OriginalTitle))
            .ForMember(d => d.PosterPath, opt => opt.MapFrom(s => s.PosterPath))
            .ForMember(d => d.IsLocal, opt => opt.MapFrom(_ => false))
            .ForMember(d => d.VoteAverage, opt => opt.MapFrom(s => s.VoteAverage))
            .ForMember(d => d.Popularity, opt => opt.MapFrom(s => s.Popularity))
            .ForMember(d => d.Overview, opt => opt.Ignore());

        // PEOPLE DB -> DTO
        CreateMap<Person, SearchPersonResultDto>(MemberList.Source)
            .ForMember(d => d.Id, opt => opt.MapFrom(s => s.Id))
            .ForMember(d => d.TmdbId, opt => opt.MapFrom(s => s.TmdbId))
            .ForMember(d => d.Name, opt => opt.MapFrom(s => s.Name))
            .ForMember(d => d.ProfilePath, opt => opt.MapFrom(s => s.ProfilePictureUrl))
            .ForMember(d => d.KnownForDepartment, opt => opt.MapFrom(s => s.KnownForDepartment))
            .ForMember(d => d.IsLocal, opt => opt.MapFrom(_ => true));

        // PEOPLE TMDb -> DTO
        CreateMap<TmdbPerson, SearchPersonResultDto>(MemberList.Source)
            .ForMember(d => d.Id, opt => opt.Ignore())
            .ForMember(d => d.TmdbId, opt => opt.MapFrom(s => s.Id))
            .ForMember(d => d.Name, opt => opt.MapFrom(s => s.Name ?? s.OriginalName ?? "(Sans nom)"))
            .ForMember(d => d.ProfilePath, opt => opt.MapFrom(s => s.ProfilePath))
            .ForMember(d => d.KnownForDepartment, opt => opt.MapFrom(s => s.KnownForDepartment))
            .ForMember(d => d.IsLocal, opt => opt.MapFrom(_ => false));

        CreateMap<Person, PersonDetailsDto>()
            .ForMember(d => d.ProfilePath, opt => opt.MapFrom(s => s.ProfilePictureUrl))
            .ForMember(d => d.MovieCredits, opt => opt.Ignore());

        // MovieCast -> PersonMovieCreditDto
        CreateMap<MovieCast, PersonMovieCreditDto>()
            .ForMember(d => d.MovieId, opt => opt.MapFrom(s => s.MovieId))
            .ForMember(d => d.TmdbMovieId, opt => opt.MapFrom(s => s.Movie.TmdbId))
            .ForMember(d => d.Title, opt => opt.MapFrom(s => s.Movie.Title))
            .ForMember(d => d.PosterPath, opt => opt.MapFrom(s => s.Movie.PosterUrl))
            .ForMember(d => d.Year, opt => opt.MapFrom(s => s.Movie.Year))
            .ForMember(d => d.Character, opt => opt.MapFrom(s => s.CharacterName))
            .ForMember(d => d.Job, opt => opt.Ignore())
            .ForMember(d => d.CreditType, opt => opt.MapFrom(_ => "cast"));

        // MovieCrew -> PersonMovieCreditDto
        CreateMap<MovieCrew, PersonMovieCreditDto>()
            .ForMember(d => d.MovieId, opt => opt.MapFrom(s => s.MovieId))
            .ForMember(d => d.TmdbMovieId, opt => opt.MapFrom(s => s.Movie.TmdbId))
            .ForMember(d => d.Title, opt => opt.MapFrom(s => s.Movie.Title))
            .ForMember(d => d.PosterPath, opt => opt.MapFrom(s => s.Movie.PosterUrl))
            .ForMember(d => d.Year, opt => opt.MapFrom(s => s.Movie.Year))
            .ForMember(d => d.Character, opt => opt.Ignore())
            .ForMember(d => d.Job, opt => opt.MapFrom(s => s.Job))
            .ForMember(d => d.CreditType, opt => opt.MapFrom(_ => "crew"));

        // TmdbPersonCastCredit -> PersonMovieCreditDto
        CreateMap<TmdbPersonCastCredit, PersonMovieCreditDto>()
            .ForMember(d => d.MovieId, opt => opt.MapFrom(_ => Guid.Empty))
            .ForMember(d => d.TmdbMovieId, opt => opt.MapFrom(s => s.Id))
            .ForMember(d => d.Title, opt => opt.MapFrom(s => s.Title))
            .ForMember(d => d.PosterPath, opt => opt.MapFrom(s => s.PosterPath))
            .ForMember(d => d.Year, opt => opt.MapFrom(s => TmdbMapHelpers.YearFromReleaseDate(s.ReleaseDate)))
            .ForMember(d => d.Character, opt => opt.MapFrom(s => s.Character))
            .ForMember(d => d.Job, opt => opt.Ignore())
            .ForMember(d => d.CreditType, opt => opt.MapFrom(_ => "cast"));

        // TmdbPersonCrewCredit -> PersonMovieCreditDto
        CreateMap<TmdbPersonCrewCredit, PersonMovieCreditDto>()
            .ForMember(d => d.MovieId, opt => opt.MapFrom(_ => Guid.Empty))
            .ForMember(d => d.TmdbMovieId, opt => opt.MapFrom(s => s.Id))
            .ForMember(d => d.Title, opt => opt.MapFrom(s => s.Title))
            .ForMember(d => d.PosterPath, opt => opt.MapFrom(s => s.PosterPath))
            .ForMember(d => d.Year, opt => opt.MapFrom(s => TmdbMapHelpers.YearFromReleaseDate(s.ReleaseDate)))
            .ForMember(d => d.Character, opt => opt.Ignore())
            .ForMember(d => d.Job, opt => opt.MapFrom(s => s.Job))
            .ForMember(d => d.CreditType, opt => opt.MapFrom(_ => "crew"));
    }
}