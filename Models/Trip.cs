using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TransportationManagement.Models
{
	public enum TripStatus
	{
		PLANNED,
		IN_PROGRESS,
		COMPLETED
	}

	[Index(nameof(driverId), nameof(vehicleId), nameof(origin), nameof(destination), IsUnique = true)]
	public class Trip
	{
		[Key]
		public int tripId { get; set; }

		[Required]
		public int vehicleId { get; set; }

		public int? driverId { get; set; }

		[Required]
		[StringLength(100)]
		public string origin { get; set; } = string.Empty;

		[Required]
		[StringLength(100)]
		public string destination { get; set; } = string.Empty;

		public string? plannedRoute { get; set; }

		[Required]
		public TripStatus tripStatus { get; set; } = TripStatus.IN_PROGRESS;

		
		[Display(Name = "Start Date & Time")]
		public DateTime? startDateTime { get; set; }

		[Display(Name = "End Date & Time")]
		public DateTime? endDateTime { get; set; }

		[ForeignKey("vehicleId")]
		public Vehicle? Vehicle { get; set; }

		[ForeignKey("driverId")]
		public Driver? Driver { get; set; }

		public double Fuelused { get; set; }
	}
}