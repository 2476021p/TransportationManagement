using TransportationManagement.Models;

namespace TransportationManagement.Interfaces
{
	public interface IMaintenanceRepository
	{
		Task<IEnumerable<MaintenanceRecord>> GetAllMaintenanceRecordsAsync();
		Task<MaintenanceRecord?> GetMaintenanceByIdAsync(int maintenanceId);
		Task<IEnumerable<MaintenanceRecord>> GetMaintenanceByVehicleIdAsync(int vehicleId);
		Task AddMaintenanceAsync(MaintenanceRecord record);
		Task UpdateMaintenanceAsync(MaintenanceRecord record);
		Task DeleteMaintenanceAsync(int maintenanceId);
		Task<bool> MaintenanceExistsAsync(int maintenanceId);

		Task<bool> IsVehicleOnTripAsync(int vehicleId);
		Task<bool> HasActiveMaintenanceAsync(int vehicleId);
		Task<Vehicle?> GetVehicleByIdAsync(int vehicleId);
		Task UpdateVehicleStatusAsync(int vehicleId, VehicleStatus status);
		Task SaveChangesAsync();
	}
}
