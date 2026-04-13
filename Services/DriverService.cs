using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TransportationManagement.Data;
using TransportationManagement.Models;

namespace TransportationManagement.Services
{
	public class DriverService
	{
		private readonly ApplicationDbContext _context;
		private readonly UserManager<ApplicationUser> _userManager;

		public DriverService(
			ApplicationDbContext context,
			UserManager<ApplicationUser> userManager)
		{
			_context = context;
			_userManager = userManager;
		}

		// --- LIST ALL ---
		public async Task<List<Driver>> ListAllDriversAsync()
		{
			return await _context.Drivers.ToListAsync();
		}

		// --- GET BY ID ---
		public async Task<Driver?> GetDriverByIdAsync(int id)
		{
			return await _context.Drivers
				.Include(d => d.Trips)
				.FirstOrDefaultAsync(d => d.driverId == id);
		}

		// --- CREATE DRIVER ---
		public async Task<(bool Success, string Message)> CreateDriverAsync(
			Driver driver, string email, string password)
		{
			try
			{
				var licExists = await _context.Drivers
					.AnyAsync(d => d.licenseNumber == driver.licenseNumber);
				if (licExists)
					return (false, "License number already exists.");

				var contactExists = await _context.Drivers
					.AnyAsync(d => d.contactNumber == driver.contactNumber);
				if (contactExists)
					return (false, "Contact number already exists.");

				// Create Identity user
				var user = new ApplicationUser
				{
					UserName = email,
					Email = email,
					FullName = driver.name
				};

				var result = await _userManager.CreateAsync(user, password);
				if (!result.Succeeded)
					return (false, string.Join(", ",
						result.Errors.Select(e => e.Description)));

				await _userManager.AddToRoleAsync(user, "Driver");

				// Link Identity user to driver record
				// Use whatever property your Driver model has for the user link
				driver.IsActive = true;
				await _context.Drivers.AddAsync(driver);
				await _context.SaveChangesAsync();

				return (true, "Driver created successfully.");
			}
			catch (Exception ex)
			{
				return (false, "Error creating driver: " + ex.Message);
			}
		}

		// --- UPDATE DRIVER ---
		public async Task<(bool Success, string Message)> UpdateDriverAsync(Driver driver)
		{
			try
			{
				var existingDriver = await _context.Drivers
					.FindAsync(driver.driverId);

				if (existingDriver == null)
					return (false, "Driver not found.");

				existingDriver.name = driver.name;
				existingDriver.licenseNumber = driver.licenseNumber;
				existingDriver.contactNumber = driver.contactNumber;
				existingDriver.status = driver.status;
				existingDriver.IsActive = driver.IsActive;

				_context.Drivers.Update(existingDriver);
				await _context.SaveChangesAsync();

				return (true, "Driver updated successfully.");
			}
			catch (Exception ex)
			{
				return (false, ex.Message);
			}
		}

		// --- DELETE DRIVER ---
		public async Task<(bool Success, string Message)> DeleteDriverAsync(int id)
		{
			try
			{
				var driver = await _context.Drivers.FindAsync(id);
				if (driver == null)
					return (false, "Driver not found.");

				// Block if active trip exists
				var hasActiveTrip = await _context.Trips
					.AnyAsync(t => t.driverId == id
						&& t.tripStatus == TripStatus.IN_PROGRESS);
				if (hasActiveTrip)
					return (false, "Cannot delete driver with an active trip.");

				// Nullify driverId on linked trips
				var linkedTrips = await _context.Trips
					.Where(t => t.driverId == id)
					.ToListAsync();
				foreach (var trip in linkedTrips)
					trip.driverId = null;

				_context.Drivers.Remove(driver);
				await _context.SaveChangesAsync();

				return (true, "Driver deleted successfully.");
			}
			catch (Exception ex)
			{
				return (false, ex.Message);
			}
		}

		// --- DASHBOARD DATA ---
		public async Task<List<Trip>> GetDriverDashboardDataAsync(int driverId)
		{
			return await _context.Trips
				.Include(t => t.Vehicle)
				.Where(t => t.driverId == driverId)
				.OrderByDescending(t => t.startDateTime)
				.ToListAsync();
		}

		// --- START TRIP ---
		public async Task<(bool Success, string Message)> TryStartTripAsync(Trip trip)
		{
			var driver = await _context.Drivers
				.AsNoTracking()
				.FirstOrDefaultAsync(d => d.driverId == trip.driverId);
			var vehicle = await _context.Vehicles
				.AsNoTracking()
				.FirstOrDefaultAsync(v => v.vehicleId == trip.vehicleId);

			if (driver == null || vehicle == null)
				return (false, "Driver or Vehicle data error.");

			if (driver.status == DriverStatus.ON_TRIP)
				return (false, "Driver is already on another trip!");

			if (vehicle.status == VehicleStatus.ON_TRIP
				|| vehicle.status == VehicleStatus.IN_SERVICE)
				return (false, "Vehicle is not available!");

			try
			{
				var dbDriver = await _context.Drivers.FindAsync(trip.driverId);
				var dbVehicle = await _context.Vehicles.FindAsync(trip.vehicleId);

				if (dbDriver == null || dbVehicle == null)
					return (false, "DB lookup error.");

				dbDriver.status = DriverStatus.ON_TRIP;
				dbVehicle.status = VehicleStatus.ON_TRIP;

				_context.Trips.Add(trip);
				await _context.SaveChangesAsync();

				return (true, "Trip started successfully!");
			}
			catch (Exception ex)
			{
				return (false, "DB Error: " + ex.Message);
			}
		}
	}
}
