using AutoMapper;
using MovieTrackR.Application.DTOs;
using MovieTrackR.Domain.Entities;

namespace MovieTrackR.Application.Mapping;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
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
        CreateMap<User, UserDto>().ReverseMap();
        // CreateMap<UserProposal, UserProposalDto>()
        //     .ForMember(dest => dest.Platform, opt => opt.MapFrom(src => src.Platform)).ReverseMap();
    }
}