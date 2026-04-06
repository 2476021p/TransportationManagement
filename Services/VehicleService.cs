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

        public async Task<IEnumerable<Vehicle>> GetAllVehiclesAsync()
        {
            return await _vehicleRepository.GetAllVehiclesAsync();
        }

        public async Task<Vehicle?> GetVehicleDetailsAsync(int vehicleId)
        {
            return await _vehicleRepository.GetVehicleByIdAsync(vehicleId);
        }

        public async Task AddVehicleAsync(Vehicle vehicle)
        {
            await _vehicleRepository.AddVehicleAsync(vehicle);
        }

        public async Task UpdateVehicleAsync(Vehicle vehicle)
        {
            await _vehicleRepository.UpdateVehicleAsync(vehicle);
        }

        public async Task DeleteVehicleAsync(int vehicleId)
        {
            await _vehicleRepository.DeleteVehicleAsync(vehicleId);
        }

        public async Task<bool> VehicleExistsAsync(int vehicleId)
        {
            return await _vehicleRepository.VehicleExistsAsync(vehicleId);
        }

        public async Task<IEnumerable<Vehicle>> ListAllVehiclesAsync()
        {
            return await _vehicleRepository.GetAllVehiclesAsync();
        }

        public async Task<bool> IsVehicleAssignedAsync(int vehicleId)
        {
            return await _vehicleRepository.IsVehicleAssignedAsync(vehicleId);
        }
    }
}
