
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TransportationManagement.Models
{
	public enum DriverStatus
	{
		AVAILABLE,
		ON_TRIP,
		INACTIVE
	}

	[Index(nameof(licenseNumber), IsUnique = true)]
	[Index(nameof(contactNumber), IsUnique = true)]
	public class Driver
	{
		[Key]
		public int driverId { get; set; }

		[Required]
		[StringLength(100)]
		public string name { get; set; } = string.Empty;

		[Required]
		[StringLength(50)]
		public string licenseNumber { get; set; } = string.Empty;

		[Required]
		[StringLength(20)]
		public string contactNumber { get; set; } = string.Empty;

		// REMOVE [Required] from status - enum has default value
		public DriverStatus status { get; set; } = DriverStatus.AVAILABLE;

		public string? UserId { get; set; }

		// MAKE User nullable with ?
		public ApplicationUser? User { get; set; }

		// Navigation properties
		public ICollection<Trip> Trips { get; set; } = new List<Trip>();
	}
}
