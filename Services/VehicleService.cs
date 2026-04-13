using TransportationManagement.Interfaces;
using TransportationManagement.Models;

namespace TransportationManagement.Services
{
	public class VehicleService
	{
		private readonly IVehicleRepository _vehicleRepository;

		public VehicleService(IVehicleRepository vehicleRepository)
		{
			_vehicleRepository = vehicleRepository;
		}

		public async Task<IEnumerable<Vehicle>> ListAllVehiclesAsync() =>
			await _vehicleRepository.GetAllVehiclesAsync();

		public async Task<Vehicle?> GetVehicleDetailsAsync(int id) =>
			await _vehicleRepository.GetVehicleByIdAsync(id);

		public async Task<(bool Success, string Message)> AddVehicleAsync(Vehicle vehicle)
		{
			var existing = await _vehicleRepository.GetByVehicleNumberAsync(vehicle.vehicleNumber);
			if (existing != null)
				return (false, $"Vehicle number '{vehicle.vehicleNumber}' already exists!");

			await _vehicleRepository.AddVehicleAsync(vehicle);
			return (true, "Vehicle added successfully!");
		}

		public async Task<(bool Success, string Message)> UpdateVehicleAsync(Vehicle vehicle)
		{
			var existing = await _vehicleRepository.GetByVehicleNumberAsync(vehicle.vehicleNumber);

			if (existing != null && existing.vehicleId != vehicle.vehicleId)
			{
				return (false, $"Vehicle number '{vehicle.vehicleNumber}' already exists!");
			}

			await _vehicleRepository.UpdateVehicleAsync(vehicle);
			return (true, "Vehicle updated successfully.");
		}

		public async Task<(bool Success, string Message)> DeleteVehicleAsync(int id)
		{
			if (await _vehicleRepository.IsVehicleAssignedAsync(id))
				return (false, "Cannot delete vehicle: It is associated with trip records.");

			if (await _vehicleRepository.HasMaintenanceRecordsAsync(id))
				return (false, "Cannot delete vehicle: It has maintenance history.");

			await _vehicleRepository.DeleteVehicleAsync(id);
			return (true, "Vehicle deleted successfully!");
		}

		// Renamed specifically to fix your error
		public async Task UpdateStatusAsync(int id, VehicleStatus status)
		{
			var vehicle = await _vehicleRepository.GetVehicleByIdAsync(id);
			if (vehicle != null)
			{
				vehicle.status = status;
				await _vehicleRepository.SaveChangesAsync();
			}
		}
	}
}