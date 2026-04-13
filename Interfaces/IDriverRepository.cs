using TransportationManagement.Models;

namespace TransportationManagement.Interfaces
{
	public interface IDriverRepository
	{
		Task<IEnumerable<Driver>> GetAllDriversAsync();
		Task<Driver?> GetDriverByIdAsync(int id);
		Task AddDriverAsync(Driver driver);
		Task UpdateDriverAsync(Driver driver);
		Task DeleteDriverAsync(int id);
		Task<IEnumerable<Trip>> GetTripsByDriverIdAsync(int driverId);
	}
}