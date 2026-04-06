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

        public async Task<IEnumerable<Driver>> GetAllDriversAsync()
        {
            return await _context.Drivers.ToListAsync();
        }

        public async Task<Driver?> GetDriverByIdAsync(int driverId)
        {
            return await _context.Drivers.FindAsync(driverId);
        }

        public async Task AddDriverAsync(Driver driver)
        {
            _context.Drivers.Add(driver);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateDriverAsync(Driver driver)
        {
            _context.Drivers.Update(driver);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteDriverAsync(int driverId)
        {
            var driver = await _context.Drivers.FindAsync(driverId);
            if (driver != null)
            {
                _context.Drivers.Remove(driver);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> DriverExistsAsync(int driverId)
        {
            return await _context.Drivers.AnyAsync(d => d.driverId == driverId);
        }

        public async Task<bool> DriverAssignedSync(int driverId)
        {
            return await _context.Drivers.AnyAsync(d => d.driverId == driverId);
        }

		public async Task<bool> IsDriverAssignedASync(int driverId)
		{
			return await _context.Trips.AnyAsync(t => t.driverId == driverId && t.tripStatus != TripStatus.COMPLETED);
		}

		public async Task<Driver?> GetDriverDetailsAsync(int driverId)
		{
			return await _context.Drivers.FirstOrDefaultAsync(d => d.driverId == driverId);
		}

	
	}
}
