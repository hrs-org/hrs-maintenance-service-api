using AutoMapper;
using HRS.Domain.Entities;
using HRS.Shared.Core.Dtos;

namespace HRS.API.Mappings.Profiles;

public class ItemMaintenanceProfile : Profile
{
    public ItemMaintenanceProfile()
    {
        CreateMap<ItemMaintenance, ItemMaintenanceResponseDto>()
            .ForMember(i => i.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(i => i.QuantityFixed, opt => opt.MapFrom(src => src.QuantityFixed))
            .ForMember(i => i.Remarks, opt => opt.MapFrom(src => src.Remarks))
            .ForMember(i => i.RentalOrderId, opt => opt.MapFrom(src => src.RentalOrderId))
            .ForMember(i => i.Type, opt => opt.MapFrom(src => src.Type.ToString()));
    }
}
