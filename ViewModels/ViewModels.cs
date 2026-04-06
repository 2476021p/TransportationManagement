using System.ComponentModel.DataAnnotations;
using TransportationManagement.Models;

namespace TransportationManagement.ViewModels
{
    public class LoginViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        public bool RememberMe { get; set; }
    }

    public class AdminDashboardViewModel
    {
        public int TotalVehicles { get; set; }
        public int TotalDrivers { get; set; }
        public int TotalTrips { get; set; }
        public int TotalMaintenanceRecords { get; set; }
        public int TotalFuelEntries { get; set; }
        public int TotalUsers { get; set; }
        public List<Vehicle> RecentVehicles { get; set; } = new();
        public List<Trip> RecentTrips { get; set; } = new();
        public List<MaintenanceRecord> RecentMaintenance { get; set; } = new();
    }

    public class UserWithRoleViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }

    public class CreateUserViewModel
    {
        [Required]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        [MinLength(6)]
        public string Password { get; set; } = string.Empty;

        [Required]
        public string Role { get; set; } = string.Empty;
    }

    public class EditUserViewModel
    {
        public string Id { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Role { get; set; } = string.Empty;
    }
}
