using Microsoft.EntityFrameworkCore;
using TransportationManagement.Data;
using TransportationManagement.Interfaces;
using TransportationManagement.Models;

namespace TransportationManagement.Repositories
{
	public class DriverRepository : IDriverRepository
	{
		private readonly ApplicationDbContext _context;

		public DriverRepository(ApplicationDbContext context)
		{
			_context = context;
		}

		public async Task<IEnumerable<Driver>> GetAllDriversAsync() =>
			await _context.Drivers.ToListAsync();

		public async Task<Driver?> GetDriverByIdAsync(int id) =>
			await _context.Drivers.FindAsync(id);

		public async Task AddDriverAsync(Driver driver)
		{
			await _context.Drivers.AddAsync(driver);
			await _context.SaveChangesAsync();
		}

		public async Task UpdateDriverAsync(Driver driver)
		{
			_context.Drivers.Update(driver);
			await _context.SaveChangesAsync();
		}

		public async Task DeleteDriverAsync(int id)
		{
			var driver = await _context.Drivers.FindAsync(id);
			if (driver != null)
			{
				_context.Drivers.Remove(driver);
				await _context.SaveChangesAsync();
			}
		}

		public async Task<IEnumerable<Trip>> GetTripsByDriverIdAsync(int driverId) =>
			await _context.Trips
				.Where(t => t.driverId == driverId)
				.ToListAsync();
	}
}
