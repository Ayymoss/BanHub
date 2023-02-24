using BanHub.WebCore.Server.Models.Context;
using BanHub.WebCore.Shared.DTOs;

namespace BanHub.WebCore.Server.Models.AutoMappers;

public class PenaltyMapping : AutoMapper.Profile
{
    public PenaltyMapping()
    {
        CreateMap<EFPenalty, PenaltyDto>()
            .ForMember(dest => dest.PenaltyType, opt => opt.MapFrom(src => src.PenaltyType))
            .ForMember(dest => dest.PenaltyStatus, opt => opt.MapFrom(src => src.PenaltyStatus))
            .ForMember(dest => dest.PenaltyScope, opt => opt.MapFrom(src => src.PenaltyScope))
            .ForMember(dest => dest.PenaltyGuid, opt => opt.MapFrom(src => src.PenaltyGuid))
            .ForMember(dest => dest.Duration, opt => opt.MapFrom(src => src.Duration))
            .ForMember(dest => dest.Submitted, opt => opt.MapFrom(src => src.Submitted))
            .ForMember(dest => dest.Reason, opt => opt.MapFrom(src => src.Reason))
            .ForMember(dest => dest.AntiCheatReason, opt => opt.MapFrom(src => src.AntiCheatReason))
            .ForMember(dest => dest.Evidence, opt => opt.MapFrom(src => src.Evidence))
            .ForMember(dest => dest.Admin, opt => opt.MapFrom(src => src.Admin))
            .ForMember(dest => dest.Target, opt => opt.MapFrom(src => src.Target))
            .ForMember(dest => dest.Instance, opt => opt.MapFrom(src => src.Instance))
            .ReverseMap();
    }
}
