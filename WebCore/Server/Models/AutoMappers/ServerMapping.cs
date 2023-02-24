using BanHub.WebCore.Server.Models.Context;
using BanHub.WebCore.Shared.DTOs;

namespace BanHub.WebCore.Server.Models.AutoMappers;

public class ServerMapping : AutoMapper.Profile
{
    public ServerMapping()
    {
        CreateMap<EFServer, ServerDto>()
            .ForMember(dest => dest.ServerId, opt => opt.MapFrom(src => src.ServerId))
            .ForMember(dest => dest.ServerName, opt => opt.MapFrom(src => src.ServerName))
            .ForMember(dest => dest.ServerIp, opt => opt.MapFrom(src => src.ServerIp))
            .ForMember(dest => dest.ServerPort, opt => opt.MapFrom(src => src.ServerPort))
            .ForMember(dest => dest.ServerGame, opt => opt.MapFrom(src => src.ServerGame))
            .ForMember(dest => dest.Connected, opt => opt.Ignore())
            .ForMember(dest => dest.Updated, opt => opt.MapFrom(src => src.Updated))
            .ForMember(dest => dest.Instance, opt => opt.MapFrom(src => src.Instance))
            .ReverseMap();
    }
}
