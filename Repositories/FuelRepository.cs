using Microsoft.EntityFrameworkCore;
using TransportationManagement.Data;
using TransportationManagement.Interfaces;
using TransportationManagement.Models;

namespace TransportationManagement.Repositories
{
	public class FuelRepository : IFuelRepository
	{
		private readonly ApplicationDbContext _context;

		public FuelRepository(ApplicationDbContext context)
		{
			_context = context;
		}

		public async Task<IEnumerable<FuelEntry>> GetAllFuelEntriesAsync() =>
			await _context.FuelEntries.Include(f => f.Vehicle).ToListAsync();

		public async Task<FuelEntry?> GetFuelEntryByIdAsync(int fuelId) =>
			await _context.FuelEntries.Include(f => f.Vehicle).FirstOrDefaultAsync(f => f.fuelId == fuelId);

		public async Task<IEnumerable<FuelEntry>> GetFuelEntriesByVehicleIdAsync(int vehicleId) =>
			await _context.FuelEntries.Include(f => f.Vehicle).Where(f => f.vehicleId == vehicleId).ToListAsync();

		public async Task AddFuelEntryAsync(FuelEntry fuelEntry) => await _context.FuelEntries.AddAsync(fuelEntry);

		public async Task UpdateFuelEntryAsync(FuelEntry fuelEntry)
		{
			_context.FuelEntries.Update(fuelEntry);
			await _context.SaveChangesAsync();
		}

		public async Task DeleteFuelEntryAsync(int fuelId)
		{
			var entry = await _context.FuelEntries.FindAsync(fuelId);
			if (entry != null)
			{
				_context.FuelEntries.Remove(entry);
				await _context.SaveChangesAsync();
			}
		}

		public async Task<bool> FuelEntryExistsAsync(int fuelId) => await _context.FuelEntries.AnyAsync(f => f.fuelId == fuelId);

		public async Task SaveChangesAsync() => await _context.SaveChangesAsync();
	}
}