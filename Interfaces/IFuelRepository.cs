using TransportationManagement.Models;

namespace TransportationManagement.Interfaces
{
	public interface IFuelRepository
	{
		Task<IEnumerable<FuelEntry>> GetAllFuelEntriesAsync();
		Task<FuelEntry?> GetFuelEntryByIdAsync(int fuelId);
		Task<IEnumerable<FuelEntry>> GetFuelEntriesByVehicleIdAsync(int vehicleId);
		Task AddFuelEntryAsync(FuelEntry fuelEntry);
		Task UpdateFuelEntryAsync(FuelEntry fuelEntry);
		Task DeleteFuelEntryAsync(int fuelId);
		Task<bool> FuelEntryExistsAsync(int fuelId);
		Task SaveChangesAsync();
	}
}
