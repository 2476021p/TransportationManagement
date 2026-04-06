using TransportationManagement.Models;

namespace TransportationManagement.Interfaces
{
    public interface IDriverRepository
    {
        Task<IEnumerable<Driver>> GetAllDriversAsync();
        Task<Driver?> GetDriverByIdAsync(int driverId);
        Task AddDriverAsync(Driver driver);
        Task UpdateDriverAsync(Driver driver);
        Task DeleteDriverAsync(int driverId);
        Task<bool> DriverExistsAsync(int driverId);
		Task<bool> IsDriverAssignedASync(int driverId);

        Task<Driver?> GetDriverDetailsAsync(int driverId);
	}
}
