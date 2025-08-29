using AutoMapper;
using DemoEcommerce.Application.DTOs;
using DemoEcommerce.Domain.Entities;

namespace DemoEcommerce.Application
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Product, ProductResponse>();
            CreateMap<Review, ReviewResponse>();
        }
    }
}
