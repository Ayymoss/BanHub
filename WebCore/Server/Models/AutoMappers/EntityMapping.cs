using BanHub.WebCore.Server.Models.Context;
using BanHub.WebCore.Shared.DTOs;

namespace BanHub.WebCore.Server.Models.AutoMappers;

public class EntityMapping : AutoMapper.Profile
{
    public EntityMapping()
    {
        CreateMap<EFEntity, EntityDto>()
            .ForMember(dest => dest.Identity, opt => opt.MapFrom(src => src.Identity))
            .ForMember(dest => dest.HeartBeat, opt => opt.MapFrom(src => src.HeartBeat))
            .ForMember(dest => dest.PlayTime, opt => opt.MapFrom(src => src.PlayTime))
            .ForMember(dest => dest.TotalConnections, opt => opt.MapFrom(src => src.TotalConnections))
            .ForMember(dest => dest.Created, opt => opt.MapFrom(src => src.Created))
            .ForMember(dest => dest.InstanceRole, opt => opt.MapFrom(src => src.InstanceRole))
            .ForMember(dest => dest.Alias, opt => opt.MapFrom(src => src.CurrentAlias))
            .ForMember(dest => dest.Notes, opt => opt.MapFrom(src => src.Notes))
            .ForMember(dest => dest.Penalties, opt => opt.MapFrom(src => src.Penalties))
            .ForMember(dest => dest.Servers, opt => opt.MapFrom(src => src.ServerConnections))
            .ReverseMap();
    }
}
