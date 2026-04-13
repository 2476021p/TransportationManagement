using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TransportationManagement.Models
{
	public enum DriverStatus
	{
		AVAILABLE,
		ON_TRIP,
		INACTIVE
	}

	public class Driver
	{
		[Key]
		public int driverId { get; set; }

		[Required(ErrorMessage = "Driver name is required")]
		[StringLength(100)]
		public string name { get; set; } = string.Empty;

		[Required(ErrorMessage = "License number is required")]
		[StringLength(20)]
		[RegularExpression(@"^[A-Za-z0-9]+$", ErrorMessage = "Only letters and numbers allowed")]
		public string licenseNumber { get; set; } = string.Empty;

		[Required(ErrorMessage = "Contact number is required")]
		[MaxLength(10)]
		[RegularExpression(@"^\+?\d{10,15}$", ErrorMessage = "Enter valid contact number")]
		public string contactNumber { get; set; } = string.Empty;

		[Required]
		public DriverStatus status { get; set; } = DriverStatus.AVAILABLE;
		public string? userId { get; set; }


		[ForeignKey("userId")]
		public ApplicationUser? User { get; set; }

		public ICollection<Trip>? Trips { get; set; }
		public ICollection<MaintenanceRecord>? MaintenanceRecords { get; set; }
		public ICollection<FuelEntry>? FuelEntries { get; set; }

		public bool IsActive { get; set; } = true;

		public string? Email { get; set; }


	}
};