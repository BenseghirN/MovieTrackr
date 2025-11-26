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
            .ForMember(d => d.Genres, m => m.MapFrom(s => s.MovieGenres.Select(mg => mg.Genre)))
            .ForMember(d => d.Cast, m => m.MapFrom(s => s.Cast.OrderBy(c => c.Order).Take(15)))
            .ForMember(d => d.Crew, m => m.MapFrom(s =>
                s.Crew
                    .Where(c => CrewHelpers.IsImportantJob(c.Job))
                    .OrderBy(c => CrewHelpers.GetJobPriority(c.Job))
                    .Take(10)));

        CreateMap<MovieCast, CastMemberDto>()
            .ForMember(d => d.Name, m => m.MapFrom(s => s.Person.Name))
            .ForMember(d => d.Character, m => m.MapFrom(s => s.CharacterName))
            .ForMember(d => d.ProfilePath, m => m.MapFrom(s => s.Person.ProfilePictureUrl));

        CreateMap<MovieCrew, CrewMemberDto>()
            .ForMember(d => d.Name, m => m.MapFrom(s => s.Person.Name))
            .ForMember(d => d.ProfilePath, m => m.MapFrom(s => s.Person.ProfilePictureUrl));

        CreateMap<UserList, UserListDto>()
            .ForMember(d => d.MoviesCount, o => o.MapFrom(s => s.Movies.Count));
        CreateMap<UserList, UserListDetailsDto>()
            .ForMember(d => d.Movies, o => o.MapFrom(
                s => s.Movies.OrderBy(m => m.Position)));
        CreateMap<Genre, GenreDto>();

        // Review -> List item
        CreateMap<Review, ReviewListItemDto>()
            .ForMember(d => d.LikesCount, opt => opt.MapFrom(s => s.Likes.Count))
            .ForMember(d => d.CommentsCount, opt => opt.MapFrom(s => s.Comments.Count));

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

        CreateMap<ReviewComment, CommentDto>();

        // DB -> DTO
        CreateMap<Movie, SearchMovieResultDto>(MemberList.Source)
            .ForMember(d => d.LocalId, o => o.MapFrom(s => s.Id))
            .ForMember(d => d.TmdbId, o => o.MapFrom(s => s.TmdbId))
            .ForMember(d => d.Title, o => o.MapFrom(s => s.Title))
            .ForMember(d => d.Year, o => o.MapFrom(s => s.ReleaseDate.HasValue ? (int?)s.ReleaseDate.Value.Year : null))
            .ForMember(d => d.OriginalTitle, o => o.MapFrom(s => s.OriginalTitle))
            .ForMember(d => d.PosterPath, o => o.MapFrom(s => s.PosterUrl)) // ajuste si c'est PosterUrl côté entity
            .ForMember(d => d.IsLocal, o => o.MapFrom(_ => true))
            .ForMember(d => d.Overview, o => o.MapFrom(s => s.Overview));

        // TMDb -> DTO
        CreateMap<TmdbSearchMovieItem, SearchMovieResultDto>(MemberList.Source)
            .ForMember(d => d.LocalId, o => o.Ignore())
            .ForMember(d => d.TmdbId, o => o.MapFrom(s => s.Id))
            .ForMember(d => d.Title, o => o.MapFrom(s => s.Title ?? s.OriginalTitle ?? "(Sans titre)"))
            .ForMember(d => d.Year, o => o.MapFrom(s => TmdbMapHelpers.YearFromReleaseDate(s.ReleaseDate)))
            .ForMember(d => d.OriginalTitle, o => o.MapFrom(s => s.OriginalTitle))
            .ForMember(d => d.PosterPath, o => o.MapFrom(s => s.PosterPath))
            .ForMember(d => d.IsLocal, o => o.MapFrom(_ => false))
            .ForMember(d => d.VoteAverage, o => o.MapFrom(s => s.VoteAverage))
            .ForMember(d => d.Popularity, o => o.MapFrom(s => s.Popularity))
            .ForMember(d => d.Overview, o => o.Ignore());
    }
}