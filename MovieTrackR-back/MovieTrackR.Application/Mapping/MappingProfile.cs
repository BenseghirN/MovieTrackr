using AutoMapper;
using MovieTrackR.Application.DTOs;
using MovieTrackR.Domain.Entities;
using MovieTrackR.Domain.Enums;

namespace MovieTrackR.Application.Mapping;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<User, UserDto>();
        CreateMap<Movie, MovieDto>()
            .ForMember(d => d.Genres, m => m.MapFrom(s => s.MovieGenres.Select(mg => mg.Genre.Name)));
        CreateMap<UserList, UserListDto>()
            .ForMember(d => d.MoviesCount, o => o.MapFrom(s => s.Movies.Count));
        CreateMap<UserList, UserListDetailsDto>()
            .ForMember(d => d.Movies, o => o.MapFrom(
                s => s.Movies.OrderBy(m => m.Position)));

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
    }
}