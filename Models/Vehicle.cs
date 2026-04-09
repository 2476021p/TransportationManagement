using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace TransportationManagement.Models
{
    public enum VehicleStatus
    {
        ACTIVE,
        IN_SERVICE,
        ON_TRIP,
        RETIRED
    }

	[Index(nameof(vehicleNumber), IsUnique = true)]


	public class Vehicle
    {
        [Key]
        public int vehicleId { get; set; }

        [Required]
        [StringLength(50)]
        public string vehicleNumber { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string model { get; set; } = string.Empty;

        [Required]
        public int capacity { get; set; }


        [Required]
        public VehicleStatus status { get; set; } = VehicleStatus.ACTIVE;

        public double currentfuel { get; set; }

        // Navigation properties
        public ICollection<Trip> Trips { get; set; } = new List<Trip>();
        public ICollection<MaintenanceRecord> MaintenanceRecords { get; set; } = new List<MaintenanceRecord>();
        public ICollection<FuelEntry> FuelEntries { get; set; } = new List<FuelEntry>();
    }
}
