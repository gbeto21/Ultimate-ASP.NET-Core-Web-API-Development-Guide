using Microsoft.EntityFrameworkCore;

namespace HotelListingAPI.Data
{
    public class HotelListingDbContext:DbContext
    {
        public HotelListingDbContext(DbContextOptions options) : base(options) {}
        
        /*Will turn those two into tables: Hotels and Countries tables*/
        public DbSet<Hotel> Hotels {  get; set; }

        public DbSet<Country> Countries {  get; set; }

    }
}
