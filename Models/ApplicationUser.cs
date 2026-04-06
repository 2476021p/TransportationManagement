using Microsoft.AspNetCore.Identity;

namespace TransportationManagement.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; } = string.Empty;

        public int? driverId { get; set; }


    }
}
