using System.Net;
using Microsoft.EntityFrameworkCore;
using TransportationManagement.Interfaces;
using TransportationManagement.Models;
using TransportationManagement.Data;

namespace TransportationManagement.Repositories
{
	public class TripRepository : ITripRepository
	{
		private readonly ApplicationDbContext _context; // ✅ FIXED

		public TripRepository(ApplicationDbContext context) // ✅ FIXED
		{
			_context = context;
		}

		public async Task<IEnumerable<Trip>> GetAllTripsAsync()
		{
			return await _context.Trips
				.Include(t => t.Vehicle)
				.Include(t => t.Driver)
				.ToListAsync();
		}

		public async Task<Trip?> GetTripByIdAsync(int tripId)
		{
			return await _context.Trips
				.Include(t => t.Vehicle)
				.Include(t => t.Driver)
				.FirstOrDefaultAsync(t => t.tripId == tripId);
		}

		public async Task<IEnumerable<Trip>> GetTripsByDriverIdAsync(int driverId)
		{
			return await _context.Trips
				.Include(t => t.Vehicle)
				.Where(t => t.driverId == driverId)
				.ToListAsync();
		}

		public async Task AddTripAsync(Trip trip)
		{
			_context.Trips.Add(trip);
			await _context.SaveChangesAsync();
		}

		public async Task UpdateTripAsync(Trip trip)
		{
			_context.Trips.Update(trip);
			await _context.SaveChangesAsync();
		}

		public async Task DeleteTripAsync(int tripId)
		{
			var trip = await _context.Trips.FindAsync(tripId);
			if (trip != null)
			{
				_context.Trips.Remove(trip);
				await _context.SaveChangesAsync();
			}
		}

		public async Task<bool> TripExistsAsync(int tripId)
		{
			return await _context.Trips.AnyAsync(t => t.tripId == tripId);
		}

		public async Task<bool> IsDriverBusyAsync(int driverId)
		{
			return await _context.Trips
				.AnyAsync(t => t.driverId == driverId
					&& t.tripStatus == TripStatus.IN_PROGRESS);
		}

		public async Task<bool> IsVehicleBusyAsync(int vehicleId)
		{
			return await _context.Trips
				.AnyAsync(t => t.vehicleId == vehicleId
					&& t.tripStatus == TripStatus.IN_PROGRESS);
		}
	}
}
