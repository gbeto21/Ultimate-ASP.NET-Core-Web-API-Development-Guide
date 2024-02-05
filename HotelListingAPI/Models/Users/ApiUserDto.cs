using System.ComponentModel.DataAnnotations;

namespace HotelListingAPI.Models.Users
{
    public class ApiUserDto : LoginDto
    {
        [Required]
        public string FirstNam { get; set; }
        [Required]
        public string LastName { get; set; }
    }
}
