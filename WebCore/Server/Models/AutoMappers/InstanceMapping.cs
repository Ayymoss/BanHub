using BanHub.WebCore.Server.Models.Context;
using BanHub.WebCore.Shared.DTOs;

namespace BanHub.WebCore.Server.Models.AutoMappers;

public class InstanceMapping : AutoMapper.Profile
{
    public InstanceMapping()
    {
        CreateMap<EFInstance, InstanceDto>()
            .ForMember(dest => dest.InstanceGuid, opt => opt.MapFrom(src => src.InstanceGuid))
            .ForMember(dest => dest.InstanceIp, opt => opt.MapFrom(src => src.InstanceIp))
            .ForMember(dest => dest.InstanceName, opt => opt.MapFrom(src => src.InstanceName))
            .ForMember(dest => dest.HeartBeat, opt => opt.MapFrom(src => src.HeartBeat))
            .ForMember(dest => dest.Created, opt => opt.MapFrom(src => src.Created))
            .ForMember(dest => dest.About, opt => opt.MapFrom(src => src.About))
            .ForMember(dest => dest.Socials, opt => opt.MapFrom(src => src.Socials))
            .ForMember(dest => dest.Active, opt => opt.MapFrom(src => src.Active))
            .ForMember(dest => dest.ServerCount, opt => opt.MapFrom(src => src.ServerConnections.Count))
            .ForMember(dest => dest.Servers, opt => opt.MapFrom(src => src.ServerConnections))
            .ReverseMap();
    }
}
