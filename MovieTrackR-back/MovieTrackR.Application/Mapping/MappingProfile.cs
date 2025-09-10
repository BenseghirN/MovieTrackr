using AutoMapper;
using MovieTrackR.Application.DTOs;
using MovieTrackR.Domain.Entities;

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

        // CreateMap<Game, GameDto>()
        //     .ForMember(dest => dest.Tags, opt => opt.MapFrom(src =>
        //         src.GameTags.Select(gt => new TagDto
        //         {
        //             Id = gt.Tag.Id,
        //             Nom = gt.Tag.Nom
        //         }).ToList()
        //     ))
        //     .ForMember(dest => dest.Platforms, opt => opt.MapFrom(src =>
        //         src.GamePlatforms.Select(gp => new PlatformDto
        //         {
        //             Id = gp.Platform.Id,
        //             Nom = gp.Platform.Nom,
        //             ImagePath = gp.Platform.ImagePath
        //         }).ToList()
        //     ))
        //     .ReverseMap()
        //     .ForMember(dest => dest.GameTags, opt => opt.Ignore())
        //     .ForMember(dest => dest.GamePlatforms, opt => opt.Ignore())
        //     .ForMember(dest => dest.UserGames, opt => opt.Ignore());
        // CreateMap<UserProposal, UserProposalDto>()
        //     .ForMember(dest => dest.Platform, opt => opt.MapFrom(src => src.Platform)).ReverseMap();
    }
}