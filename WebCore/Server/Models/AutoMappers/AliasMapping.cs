using BanHub.WebCore.Server.Models.Context;
using BanHub.WebCore.Shared.DTOs;

namespace BanHub.WebCore.Server.Models.AutoMappers;

public class AliasMapping : AutoMapper.Profile
{
    public AliasMapping()
    {
        CreateMap<EFAlias, AliasDto>()
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.UserName))
            .ForMember(dest => dest.IpAddress, opt => opt.MapFrom(src => src.IpAddress))
            .ForMember(dest => dest.Changed, opt => opt.MapFrom(src => src.Changed));
    }
}
