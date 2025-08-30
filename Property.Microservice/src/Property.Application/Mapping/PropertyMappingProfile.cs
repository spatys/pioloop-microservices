using AutoMapper;
using Property.Application.DTOs.Response;
using Property.Domain.Entities;
using System.Text;

namespace Property.Application.Mapping;

public class PropertyMappingProfile : Profile
{
    public PropertyMappingProfile()
    {
        CreateMap<Property.Domain.Entities.Property, PropertyResponse>()
                                    .ForMember(dest => dest.Images, opt => opt.MapFrom(src => 
                            src.Images.OrderBy(i => i.DisplayOrder).Select(img => new PropertyImageResponse
                            {
                                Id = img.Id,
                                ImageUrl = img.ImageUrl,
                                AltText = img.AltText,
                                IsMainImage = img.IsMainImage,
                                DisplayOrder = img.DisplayOrder
                            })))
            .ForMember(dest => dest.Amenities, opt => opt.MapFrom(src => 
                src.Amenities.OrderBy(a => a.DisplayOrder).Select(a => a.Name)))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));

        CreateMap<Property.Domain.Entities.PropertyImage, PropertyImageResponse>()
            .ForMember(dest => dest.ImageUrl, opt => opt.MapFrom(src => src.ImageUrl));
    }
}
