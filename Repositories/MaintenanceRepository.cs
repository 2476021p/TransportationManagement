using Microsoft.EntityFrameworkCore;
using TransportationManagement.Data;
using TransportationManagement.Interfaces;
using TransportationManagement.Models;

namespace TransportationManagement.Repositories
{
	public class MaintenanceRepository : IMaintenanceRepository
	{
		private readonly ApplicationDbContext _context;

		public MaintenanceRepository(ApplicationDbContext context)
		{
			_context = context;
		}

		public async Task<IEnumerable<MaintenanceRecord>> GetAllMaintenanceRecordsAsync() =>
			await _context.MaintenanceRecords
				.Include(m => m.Vehicle)
				.ToListAsync();

		public async Task<MaintenanceRecord?> GetMaintenanceByIdAsync(int maintenanceId) =>
			await _context.MaintenanceRecords
				.Include(m => m.Vehicle)
				.FirstOrDefaultAsync(m => m.maintenanceId == maintenanceId);

		public async Task<IEnumerable<MaintenanceRecord>> GetMaintenanceByVehicleIdAsync(int vehicleId) =>
			await _context.MaintenanceRecords
				.Include(m => m.Vehicle)
				.Where(m => m.vehicleId == vehicleId)
				.ToListAsync();

		public async Task AddMaintenanceAsync(MaintenanceRecord record)
		{
			_context.MaintenanceRecords.Add(record);
			await _context.SaveChangesAsync();
		}

		public async Task UpdateMaintenanceAsync(MaintenanceRecord record)
		{
			_context.MaintenanceRecords.Update(record);
			await _context.SaveChangesAsync();
		}

		public async Task DeleteMaintenanceAsync(int maintenanceId)
		{
			var record = await _context.MaintenanceRecords.FindAsync(maintenanceId);
			if (record != null)
			{
				_context.MaintenanceRecords.Remove(record);
				await _context.SaveChangesAsync();
			}
		}

		public async Task<bool> MaintenanceExistsAsync(int maintenanceId) =>
			await _context.MaintenanceRecords
				.AnyAsync(m => m.maintenanceId == maintenanceId);

		public async Task<bool> IsVehicleOnTripAsync(int vehicleId) =>
			await _context.Trips
				.AnyAsync(t => t.vehicleId == vehicleId
					&& t.tripStatus == TripStatus.IN_PROGRESS);

		public async Task<bool> HasActiveMaintenanceAsync(int vehicleId) =>
			await _context.MaintenanceRecords
				.AnyAsync(m => m.vehicleId == vehicleId
					&& m.status != MaintenanceStatus.COMPLETED);

		public async Task<Vehicle?> GetVehicleByIdAsync(int vehicleId) =>
			await _context.Vehicles.FindAsync(vehicleId);

		public async Task UpdateVehicleStatusAsync(int vehicleId, VehicleStatus status)
		{
			var vehicle = await _context.Vehicles.FindAsync(vehicleId);
			if (vehicle != null)
			{
				vehicle.status = status;
				await _context.SaveChangesAsync();
			}
		}

		public async Task SaveChangesAsync() =>
			await _context.SaveChangesAsync();
	}
}