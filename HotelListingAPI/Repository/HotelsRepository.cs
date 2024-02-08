using AutoMapper;
using HotelListingAPI.Contracts;
using HotelListingAPI.Data;

namespace HotelListingAPI.Repository
{
    public class HotelsRepository : GenericRepository<Hotel>, IHotelsRepository
    {
        private readonly HotelListingDbContext context;

        public HotelsRepository(HotelListingDbContext context, IMapper mapper) : base(context, mapper)
        {
            this.context = context;
        }
    }
}
