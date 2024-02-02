using AutoMapper;
using HotelListingAPI.Data;
using HotelListingAPI.Models.Country;

namespace HotelListingAPI.Configurations
{
    public class MapperConfig : Profile
    {
        public MapperConfig()
        {
            CreateMap<Country, CreateContryDto>().ReverseMap();
        }
    }
}
