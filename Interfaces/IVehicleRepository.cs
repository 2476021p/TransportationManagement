using TransportationManagement.Models;

namespace TransportationManagement.Interfaces
{
	public interface IVehicleRepository
	{
		Task<IEnumerable<Vehicle>> GetAllVehiclesAsync();
		Task<Vehicle?> GetVehicleByIdAsync(int vehicleId);
		Task AddVehicleAsync(Vehicle vehicle);
		Task UpdateVehicleAsync(Vehicle vehicle);
		Task DeleteVehicleAsync(int vehicleId);
		Task<bool> VehicleExistsAsync(int vehicleId);
		Task<bool> IsVehicleAssignedAsync(int vehicleId);
		Task<bool> HasMaintenanceRecordsAsync(int vehicleId);
		Task<Vehicle?> GetByVehicleNumberAsync(string vehicleNumber);
		Task SaveChangesAsync();
	}
}