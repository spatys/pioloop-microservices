using AutoMapper;
using Property.Application.DTOs;
using PropertyEntity = Property.Domain.Entities.Property;
using PropertyImage = Property.Domain.Entities.PropertyImage;
using PropertyAmenity = Property.Domain.Entities.PropertyAmenity;
using PropertyType = Property.Domain.Entities.PropertyType;

namespace Property.Application.Mapping;

public class PropertyMappingProfile : Profile
{
    public PropertyMappingProfile()
    {
        CreateMap<PropertyEntity, PropertyDto>()
            .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type.ToString()))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));

        CreateMap<CreatePropertyDto, PropertyEntity>()
            .ForMember(dest => dest.Type, opt => opt.MapFrom(src => Enum.Parse<PropertyType>(src.Type)));

        CreateMap<PropertyImage, PropertyImageDto>();
        CreateMap<PropertyAmenity, PropertyAmenityDto>()
            .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type.ToString()))
            .ForMember(dest => dest.Category, opt => opt.MapFrom(src => src.Category.ToString()));
    }
}
