using AutoMapper;
using HRS.API.Contracts.DTOs.Maintenance;
using HRS.Domain.Entities;

namespace HRS.API.Mappings.Profiles;

public class ItemMaintenanceProfile : Profile
{
    public ItemMaintenanceProfile()
    {
        CreateMap<ItemMaintenance, ItemMaintenanceResponseDto>()
            .ForMember(i => i.Id, opt => opt.MapFrom(src => src._id != null ? src._id.GetHashCode() : 0))
            .ForMember(i => i.QuantityFixed, opt => opt.MapFrom(src => src.QuantityFixed))
            .ForMember(i => i.Remarks, opt => opt.MapFrom(src => src.Remarks))
            .ForMember(i => i.RentalOrderId, opt => opt.MapFrom(src => src.RentalOrderId))
            .ForMember(i => i.Type, opt => opt.MapFrom(src => src.Type.ToString()));
    }
}
