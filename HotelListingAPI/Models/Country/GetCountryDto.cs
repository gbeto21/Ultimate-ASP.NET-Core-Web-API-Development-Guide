using System.ComponentModel.DataAnnotations.Schema;

namespace HotelListingAPI.Models.Country
{
    public class GetCountryDto
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public string ShortName { get; set; }

    }
}
