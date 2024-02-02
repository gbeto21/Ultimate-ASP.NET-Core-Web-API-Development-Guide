using HotelListingAPI.Contracts;
using HotelListingAPI.Data;

namespace HotelListingAPI.Repository
{
    public class HotelsRepository : GenericRepository<Hotel>, IHotelsRepository
    {
        private readonly HotelListingDbContext context;

        public HotelsRepository(HotelListingDbContext context) : base(context)
        {
            this.context = context;
        }
    }
}
