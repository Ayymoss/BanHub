using BanHub.WebCore.Server.Models.Context;
using BanHub.WebCore.Shared.DTOs;

namespace BanHub.WebCore.Server.Models.AutoMappers;

public class CurrentAliasMapping : AutoMapper.Profile
{
    public CurrentAliasMapping()
    {
        CreateMap<EFCurrentAlias, AliasDto>()
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.Alias.UserName))
            .ForMember(dest => dest.IpAddress, opt => opt.MapFrom(src => src.Alias.IpAddress))
            .ForMember(dest => dest.Changed, opt => opt.MapFrom(src => src.Alias.Changed))
            .ReverseMap();
    }
}
