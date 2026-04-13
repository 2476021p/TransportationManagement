using Microsoft.EntityFrameworkCore;
using TransportationManagement.Data;
using TransportationManagement.Interfaces;
using TransportationManagement.Models;

namespace TransportationManagement.Repositories
{
	public class VehicleRepository : IVehicleRepository
	{
		private readonly ApplicationDbContext _context;

		public VehicleRepository(ApplicationDbContext context)
		{
			_context = context;
		}

		public async Task<IEnumerable<Vehicle>> GetAllVehiclesAsync() =>
			await _context.Vehicles.ToListAsync();

		public async Task<Vehicle?> GetVehicleByIdAsync(int vehicleId) =>
			await _context.Vehicles.FindAsync(vehicleId);

		public async Task AddVehicleAsync(Vehicle vehicle)
		{
			_context.Vehicles.Add(vehicle);
			await _context.SaveChangesAsync();
		}

		public async Task UpdateVehicleAsync(Vehicle vehicle)
		{
			// 1. Check if the entity is already being tracked in the local memory
			var local = _context.Vehicles
				.Local
				.FirstOrDefault(entry => entry.vehicleId == vehicle.vehicleId);

			// 2. If it is being tracked, "Detach" it so we can attach the new version
			if (local != null)
			{
				_context.Entry(local).State = EntityState.Detached;
			}

			// 3. Now update the vehicle safely
			_context.Vehicles.Update(vehicle);
			await _context.SaveChangesAsync();
		}

		public async Task DeleteVehicleAsync(int vehicleId)
		{
			var vehicle = await _context.Vehicles.FindAsync(vehicleId);
			if (vehicle != null)
			{
				_context.Vehicles.Remove(vehicle);
				await _context.SaveChangesAsync();
			}
		}

		public async Task<bool> VehicleExistsAsync(int vehicleId) =>
			await _context.Vehicles.AnyAsync(v => v.vehicleId == vehicleId);

		public async Task<bool> IsVehicleAssignedAsync(int vehicleId) =>
			await _context.Trips.AnyAsync(t => t.vehicleId == vehicleId);

		public async Task<bool> HasMaintenanceRecordsAsync(int vehicleId) =>
			await _context.MaintenanceRecords.AnyAsync(m => m.vehicleId == vehicleId);

		public async Task<Vehicle?> GetByVehicleNumberAsync(string vehicleNumber)
		{
			// Using AsNoTracking() is the KEY fix for your error
			return await _context.Vehicles
				.AsNoTracking()
				.FirstOrDefaultAsync(v => v.vehicleNumber == vehicleNumber);
		}

		public async Task SaveChangesAsync() => await _context.SaveChangesAsync();
	}
}