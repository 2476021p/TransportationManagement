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

        public async Task<IEnumerable<MaintenanceRecord>> GetAllMaintenanceRecordsAsync()
        {
            return await _maintenanceRepository.GetAllMaintenanceRecordsAsync();
        }

        public async Task<MaintenanceRecord?> GetMaintenanceByIdAsync(int maintenanceId)
        {
            return await _maintenanceRepository.GetMaintenanceByIdAsync(maintenanceId);
        }

        public async Task ScheduleMaintenanceAsync(MaintenanceRecord record)
        {
            await _maintenanceRepository.AddMaintenanceAsync(record);
        }

        public async Task UpdateServiceRecordAsync(MaintenanceRecord record)
        {
            await _maintenanceRepository.UpdateMaintenanceAsync(record);
        }

        public async Task<IEnumerable<MaintenanceRecord>> GetMaintenanceHistoryAsync(int vehicleId)
        {
            return await _maintenanceRepository.GetMaintenanceByVehicleIdAsync(vehicleId);
        }

        public async Task DeleteMaintenanceAsync(int maintenanceId)
        {
            await _maintenanceRepository.DeleteMaintenanceAsync(maintenanceId);
        }

        public async Task<bool> MaintenanceExistsAsync(int maintenanceId)
        {
            return await _maintenanceRepository.MaintenanceExistsAsync(maintenanceId);
        }
    }
}
