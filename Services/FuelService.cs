using TransportationManagement.Interfaces;
using TransportationManagement.Models;

namespace TransportationManagement.Services
{
	public class FuelService
	{
		private readonly IFuelRepository _fuelRepository;
		private readonly IVehicleRepository _vehicleRepository;

		public FuelService(
			IFuelRepository fuelRepository,
			IVehicleRepository vehicleRepository)
		{
			_fuelRepository = fuelRepository;
			_vehicleRepository = vehicleRepository;
		}

		public async Task<IEnumerable<FuelEntry>> GetAllFuelEntriesAsync() =>
			await _fuelRepository.GetAllFuelEntriesAsync();

		public async Task<FuelEntry?> GetFuelEntryByIdAsync(int fuelId) =>
			await _fuelRepository.GetFuelEntryByIdAsync(fuelId);

		public async Task<(bool Success, string Message)> AddFuelEntryAsync(
			FuelEntry fuelEntry)
		{
			var vehicle = await _vehicleRepository
				.GetVehicleByIdAsync(fuelEntry.vehicleId);
			if (vehicle == null)
				return (false, "Vehicle not found.");

			await _fuelRepository.AddFuelEntryAsync(fuelEntry);

			// Update vehicle fuel level
			vehicle.currentfuel += (double)fuelEntry.fuelQuantity;
			await _fuelRepository.SaveChangesAsync();

			return (true, "Fuel added successfully.");
		}

		public async Task<IEnumerable<FuelEntry>> GetFuelConsumptionAsync(
			int vehicleId) =>
			await _fuelRepository.GetFuelEntriesByVehicleIdAsync(vehicleId);

		public async Task<IEnumerable<FuelEntry>> GenerateFuelReportAsync() =>
			await _fuelRepository.GetAllFuelEntriesAsync();

		public async Task UpdateFuelEntryAsync(FuelEntry fuelEntry) =>
			await _fuelRepository.UpdateFuelEntryAsync(fuelEntry);

		public async Task DeleteFuelEntryAsync(int fuelId) =>
			await _fuelRepository.DeleteFuelEntryAsync(fuelId);
	}
}
