using BanHub.WebCore.Server.Models.Context;
using BanHub.WebCore.Shared.DTOs;

namespace BanHub.WebCore.Server.Models.AutoMappers;

public class ServerConnectionMapping : AutoMapper.Profile
{
    public ServerConnectionMapping()
    {
        CreateMap<EFServerConnection, ServerDto>()
            .ForMember(dest => dest.ServerId, opt => opt.MapFrom(src => src.Server.Id))
            .ForMember(dest => dest.ServerName, opt => opt.MapFrom(src => src.Server.ServerName))
            .ForMember(dest => dest.ServerIp, opt => opt.MapFrom(src => src.Server.ServerIp))
            .ForMember(dest => dest.ServerPort, opt => opt.MapFrom(src => src.Server.ServerPort))
            .ForMember(dest => dest.ServerGame, opt => opt.MapFrom(src => src.Server.ServerGame))
            .ForMember(dest => dest.Connected, opt => opt.MapFrom(src => src.Connected))
            .ForMember(dest => dest.Updated, opt => opt.Ignore())
            .ForMember(dest => dest.Instance, opt => opt.MapFrom(src => src.Server.Instance))
            .ReverseMap();
    }
}
