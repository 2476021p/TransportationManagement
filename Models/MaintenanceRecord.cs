using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TransportationManagement.Models
{
	public enum MaintenanceStatus
	{
		SCHEDULED,
		IN_SERVICE,
		COMPLETED
	}

	public class MaintenanceRecord
	{
		[Key]
		public int maintenanceId { get; set; }

		[Required]
		public int vehicleId { get; set; }

		[Required]
		[StringLength(100)]
		public string serviceType { get; set; } = string.Empty;

		[Required]
		public DateTime serviceDate { get; set; }

		[StringLength(255)]
		public string? remarks { get; set; }

		// ADD THIS - Status field
		public MaintenanceStatus status { get; set; }
			= MaintenanceStatus.SCHEDULED;

		// Navigation property
		[ForeignKey("vehicleId")]
		public Vehicle? Vehicle { get; set; }
	}
}
