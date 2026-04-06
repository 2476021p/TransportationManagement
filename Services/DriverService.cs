using TransportationManagement.Interfaces;
using TransportationManagement.Models;

namespace TransportationManagement.Services
{
	public class DriverService
	{
		private readonly IDriverRepository _driverRepository;

		public DriverService(IDriverRepository driverRepository)
		{
			_driverRepository = driverRepository;
		}

		public async Task<IEnumerable<Driver>> GetAllDriversAsync()
		{
			return await _driverRepository.GetAllDriversAsync();
		}

		// ✅ FIXED NAME (IMPORTANT)
		public async Task<Driver?> GetDriverByIdAsync(int driverId)
		{
			return await _driverRepository.GetDriverByIdAsync(driverId);
		}

		public async Task AddDriverAsync(Driver driver)
		{
			await _driverRepository.AddDriverAsync(driver);
		}

		public async Task UpdateDriverAsync(Driver driver)
		{
			await _driverRepository.UpdateDriverAsync(driver);
		}

		public async Task DeleteDriverAsync(int driverId)
		{
			await _driverRepository.DeleteDriverAsync(driverId);
		}

		public async Task<bool> DriverExistsAsync(int driverId)
		{
			return await _driverRepository.DriverExistsAsync(driverId);
		}

		public async Task<IEnumerable<Driver>> GetAssignedTripsDriversAsync()
		{
			return await _driverRepository.GetAllDriversAsync();
		}

		// ✅ FIXED METHOD
		public async Task<bool> IsDriverAssignedAsync(int driverId)
		{
			return await _driverRepository.IsDriverAssignedASync(driverId);
		}


		public async Task<Driver?> GetDriverDetailsAsync(int driverId)
		{
			return await _driverRepository.GetDriverByIdAsync(driverId);
		}
		// ❌ REMOVED DUPLICATE METHOD
		// GetDriverAssignedAsync ❌
	}
}