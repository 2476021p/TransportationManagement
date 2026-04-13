using TransportationManagement.Interfaces;
using TransportationManagement.Models;

namespace TransportationManagement.Services
{
	public class MaintenanceService
	{
		private readonly IMaintenanceRepository _maintenanceRepository;

		public MaintenanceService(IMaintenanceRepository maintenanceRepository)
		{
			_maintenanceRepository = maintenanceRepository;
		}

		public async Task<IEnumerable<MaintenanceRecord>> GetAllMaintenanceRecordsAsync() =>
			await _maintenanceRepository.GetAllMaintenanceRecordsAsync();

		public async Task<MaintenanceRecord?> GetMaintenanceByIdAsync(int id) =>
			await _maintenanceRepository.GetMaintenanceByIdAsync(id);

		public async Task<(bool Success, string Message)> ScheduleMaintenanceAsync(
			MaintenanceRecord record)
		{
			var vehicle = await _maintenanceRepository.GetVehicleByIdAsync(record.vehicleId);

			if (vehicle == null || vehicle.status == VehicleStatus.IN_SERVICE)
				return (false, "Vehicle is already on maintenance.");

			if (await _maintenanceRepository.IsVehicleOnTripAsync(record.vehicleId))
				return (false, "Cannot schedule maintenance. Vehicle is currently on a trip!");

			if (await _maintenanceRepository.HasActiveMaintenanceAsync(record.vehicleId))
				return (false, "This vehicle already has a scheduled maintenance!");

			await _maintenanceRepository.AddMaintenanceAsync(record);
			await _maintenanceRepository.UpdateVehicleStatusAsync(
				record.vehicleId, VehicleStatus.IN_SERVICE);
			await _maintenanceRepository.SaveChangesAsync();

			return (true, "Maintenance scheduled successfully!");
		}

		public async Task<(bool Success, string Message)> UpdateServiceRecordAsync(
			int id, MaintenanceRecord record)
		{
			var existing = await _maintenanceRepository.GetMaintenanceByIdAsync(id);
			if (existing == null) return (false, "Record not found.");

			if (existing.status == MaintenanceStatus.COMPLETED)
				return (false, "Completed maintenance cannot be updated.");

			existing.serviceType = record.serviceType;
			existing.serviceDate = record.serviceDate;
			existing.remarks = record.remarks;
			existing.status = record.status;

			// If marking as COMPLETED, set vehicle back to ACTIVE
			if (record.status == MaintenanceStatus.COMPLETED)
				await _maintenanceRepository.UpdateVehicleStatusAsync(
					record.vehicleId, VehicleStatus.ACTIVE);

			await _maintenanceRepository.UpdateMaintenanceAsync(existing);
			return (true, "Service record updated successfully.");
		}

		public async Task<IEnumerable<MaintenanceRecord>> GetMaintenanceHistoryAsync(
			int vehicleId) =>
			await _maintenanceRepository.GetMaintenanceByVehicleIdAsync(vehicleId);

		public async Task<(bool Success, string Message)> DeleteMaintenanceAsync(int id)
		{
			var record = await _maintenanceRepository.GetMaintenanceByIdAsync(id);
			if (record == null) return (false, "Record not found.");

			if (record.status == MaintenanceStatus.COMPLETED)
				return (false, "Completed maintenance cannot be deleted.");

			// Reset vehicle to ACTIVE when maintenance is removed
			await _maintenanceRepository.UpdateVehicleStatusAsync(
				record.vehicleId, VehicleStatus.ACTIVE);

			await _maintenanceRepository.DeleteMaintenanceAsync(id);
			return (true, "Maintenance deleted successfully!");
		}
	}
}
