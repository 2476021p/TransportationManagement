using TransportationManagement.Interfaces;
using TransportationManagement.Models;

namespace TransportationManagement.Services
{
    public class FuelService
    {
        private readonly IFuelRepository _fuelRepository;

        public FuelService(IFuelRepository fuelRepository)
        {
            _fuelRepository = fuelRepository;
        }

        public async Task<IEnumerable<FuelEntry>> GetAllFuelEntriesAsync()
        {
            return await _fuelRepository.GetAllFuelEntriesAsync();
        }

        public async Task AddFuelEntryAsync(FuelEntry fuelEntry)
        {
            await _fuelRepository.AddFuelEntryAsync(fuelEntry);
        }

        public async Task<IEnumerable<FuelEntry>> GetFuelConsumptionAsync(int vehicleId)
        {
            return await _fuelRepository.GetFuelEntriesByVehicleIdAsync(vehicleId);
        }

        public async Task<IEnumerable<FuelEntry>> GenerateFuelReportAsync()
        {
            return await _fuelRepository.GetAllFuelEntriesAsync();
        }

        public async Task UpdateFuelEntryAsync(FuelEntry fuelEntry)
        {
            await _fuelRepository.UpdateFuelEntryAsync(fuelEntry);
        }

        public async Task DeleteFuelEntryAsync(int fuelId)
        {
            await _fuelRepository.DeleteFuelEntryAsync(fuelId);
        }

        public async Task<FuelEntry?> GetFuelEntryByIdAsync(int fuelId)
        {
            return await _fuelRepository.GetFuelEntryByIdAsync(fuelId);
        }

        public async Task<bool> FuelEntryExistsAsync(int fuelId)
        {
            return await _fuelRepository.FuelEntryExistsAsync(fuelId);
        }
    }
}
