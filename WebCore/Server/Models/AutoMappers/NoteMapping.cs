using BanHub.WebCore.Server.Models.Context;
using BanHub.WebCore.Shared.DTOs;

namespace BanHub.WebCore.Server.Models.AutoMappers;

public class NoteMapping : AutoMapper.Profile
{
    public NoteMapping()
    {
        CreateMap<EFNote, NoteDto>()
            .ForMember(dest => dest.DeletionReason, opt => opt.Ignore())
            .ForMember(dest => dest.Admin, opt => opt.MapFrom(src => src.Admin))
            .ForMember(dest => dest.Target, opt => opt.MapFrom(src => src.Target))
            .ForMember(dest => dest.NoteGuid, opt => opt.MapFrom(src => src.NoteGuid))
            .ForMember(dest => dest.Message, opt => opt.MapFrom(src => src.Message))
            .ForMember(dest => dest.IsPrivate, opt => opt.MapFrom(src => src.IsPrivate))
            .ForMember(dest => dest.Created, opt => opt.MapFrom(src => src.Created))
            .ReverseMap();
    }
}
