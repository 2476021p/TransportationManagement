using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TransportationManagement.Data;
using TransportationManagement.Migrations;
using TransportationManagement.Models;
using TransportationManagement.Services;

namespace TransportationManagement.Controllers
{
	[Authorize(Roles = "Admin,FleetManager,Driver")]
	public class TripController : Controller
	{
		private readonly TripService _tripService;
		private readonly VehicleService _vehicleService;
		private readonly DriverService _driverService;
		private readonly ApplicationDbContext _context;

		public TripController(
			TripService tripService,
			VehicleService vehicleService,
			DriverService driverService,
			ApplicationDbContext context)
		{
			_tripService = tripService;
			_vehicleService = vehicleService;
			_driverService = driverService;
			_context = context;
		}

		
		public async Task<IActionResult> Index()
		{
			try
			{
				var trips = await _tripService.GetAllTripsAsync();
				return View(trips);
			}
			catch (Exception ex)
			{
				TempData["Error"] = "An error occurred while loading trips: " + ex.Message;
				return View(new List<Trip>());
			}
		}

		
		public async Task<IActionResult> GetTripPlan(int id)
		{
			try
			{
				var trip = await _tripService.GetTripPlanAsync(id);
				if (trip == null) return NotFound();
				return View(trip);
			}
			catch (Exception ex)
			{
				TempData["Error"] = "An error occurred while loading trip details: " + ex.Message;
				return RedirectToAction(nameof(Index));
			}
		}


		[Authorize(Roles = "Admin,FleetManager")]
		public async Task<IActionResult> CreateTrip()
		{
			try
			{
				await PopulateDropdowns();
				return View();
			}
			catch (Exception ex)
			{
				TempData["Error"] = "Error loading create trip page: " + ex.Message;
				return RedirectToAction(nameof(Index));
			}
		}



		[HttpPost]
		[Authorize(Roles = "Admin,FleetManager")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> CreateTrip(Trip trip)
		{
			try
			{
				// ✅ Get vehicle
				var vehicle = await _context.Vehicles
					.FirstOrDefaultAsync(v => v.vehicleId == trip.vehicleId);

				if (vehicle == null)
				{
					ModelState.AddModelError("", "Selected vehicle not found.");
				}

				// ❌ REMOVE this check (causing issue)
				// else if (vehicle.status == VehicleStatus.IN_SERVICE)

				// ✅ Get driver
				var driver = await _context.Drivers
					.FirstOrDefaultAsync(d => d.driverId == trip.driverId);

				if (driver == null)
				{
					ModelState.AddModelError("driverId", "Selected driver not found.");
				}

				// ❌ REMOVE this check (causing issue)
				// else if (driver.status != DriverStatus.AVAILABLE)

				// ✅ ONLY CHECK ACTIVE TRIPS (MAIN FIX)
				bool driverBusy = await _context.Trips
					.AnyAsync(t => t.driverId == trip.driverId &&
								   t.tripStatus == TripStatus.IN_PROGRESS);

				bool vehicleBusy = await _context.Trips
					.AnyAsync(t => t.vehicleId == trip.vehicleId &&
								   t.tripStatus == TripStatus.IN_PROGRESS);

				if (vehicleBusy)
				{
					ModelState.AddModelError("vehicleId", "Vehicle is already assigned to another active trip.");
				}

				if (driverBusy)
				{
					ModelState.AddModelError("driverId", "Driver is already assigned to another active trip.");
				}

				// ❗ FIX: ModelState condition was wrong in your code
				if (!ModelState.IsValid)
				{
					await PopulateDropdowns();
					return View(trip);
				}

				// ✅ CREATE TRIP
				trip.tripStatus = TripStatus.PLANNED;
				trip.startDateTime = DateTime.Now;

				_context.Trips.Add(trip);

				// ✅ UPDATE STATUS
				if (vehicle != null)
					vehicle.status = VehicleStatus.ON_TRIP;

				if (driver != null)
					driver.status = DriverStatus.ON_TRIP;

				trip.startDateTime = DateTime.Now;
				trip.endDateTime = null;
				trip.tripStatus = TripStatus.PLANNED;

				await _context.SaveChangesAsync();

				TempData["Success"] = "Trip created successfully!";
				return RedirectToAction(nameof(Index));
			}
			catch (Exception ex)
			{
				var error = ex.InnerException?.Message ?? ex.Message;
				TempData["Error"] = error;
				await PopulateDropdowns();
				return View(trip);
			}
		}


		[Authorize(Roles = "Admin,FleetManager")]
		public async Task<IActionResult> Edit(int id)
		{
			try
			{
				var trip = await _tripService.GetTripPlanAsync(id);
				if (trip == null) return NotFound();
				await PopulateDropdowns();
				return View(trip);
			}
			catch (Exception ex)
			{
				TempData["Error"] = "An error occurred while loading the edit trip page: " + ex.Message;
				return RedirectToAction(nameof(Index));
			}
		}


		[HttpPost]
		[Authorize(Roles = "Admin,FleetManager")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(Trip trip)
		{
			try
			{
				var existingTrip = await _context.Trips
					.Include(t => t.Driver)
					.Include(t => t.Vehicle)
					.FirstOrDefaultAsync(t => t.tripId == trip.tripId);

				if (existingTrip == null)
					return NotFound();

				// Update basic fields
				existingTrip.origin = trip.origin;
				existingTrip.destination = trip.destination;
				existingTrip.tripStatus = trip.tripStatus;

				// 🔥 MAIN LOGIC
				if (trip.tripStatus == TripStatus.IN_PROGRESS)
				{
					if (existingTrip.Driver != null)
						existingTrip.Driver.status = DriverStatus.ON_TRIP;

					if (existingTrip.Vehicle != null)
						existingTrip.Vehicle.status = VehicleStatus.ON_TRIP;
				}
				else if (trip.tripStatus == TripStatus.COMPLETED)
				{
					existingTrip.endDateTime = DateTime.Now;

					if (existingTrip.Driver != null)
						existingTrip.Driver.status = DriverStatus.AVAILABLE;

					if (existingTrip.Vehicle != null)
						existingTrip.Vehicle.status = VehicleStatus.ACTIVE;
				}

				await _context.SaveChangesAsync();

				TempData["Success"] = "Trip updated successfully!";
				return RedirectToAction(nameof(Index));
			}
			catch (Exception ex)
			{
				TempData["Error"] = "Error updating trip: " + ex.Message;
				await PopulateDropdowns();
				return View(trip);
			}
		}


		[Authorize(Roles = "Admin,FleetManager")]
		public async Task<IActionResult> Delete(int id)
		{
			try
			{
				var trip = await _tripService.GetTripPlanAsync(id);
				if (trip == null) return NotFound();
				return View(trip);
			}
			catch (Exception ex)
			{
				TempData["Error"] = "An error occurred while loading the delete trip page: " + ex.Message;
				return RedirectToAction(nameof(Index));
			}
		}

		
		[HttpPost, ActionName("Delete")]
		[Authorize(Roles = "Admin,FleetManager")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> DeleteConfirmed(int id)
		{
			try
			{
				await _tripService.DeleteTripAsync(id);
				TempData["Success"] = "Trip deleted successfully!";
				return RedirectToAction(nameof(Index));
			}
			catch (Exception ex)
			{
				TempData["Error"] = "An error occurred while deleting the trip: " + ex.Message;
				return RedirectToAction(nameof(Index));
			}
		}

		private async Task PopulateDropdowns()
		{
			try
			{
				var vehicles = await _context.Vehicles.Where(v => v.status == VehicleStatus.ACTIVE).ToListAsync();

				var drivers = await _context.Drivers.ToListAsync();

				ViewBag.Vehicles = new SelectList(vehicles, "vehicleId", "vehicleNumber");
				ViewBag.Drivers = new SelectList(drivers, "driverId", "name");


			}
			catch (Exception ex) {
				TempData["Error"] = "Error loading dropdowns: + ex.Messsage";
			}
		}

		// ================= GET =================
		[HttpGet]
		[Authorize(Roles = "Admin,FleetManager")]
		public async Task<IActionResult> UpdateTripStatus(int id)
		{
			var trip = await _context.Trips
				.Include(t => t.Driver)
				.Include(t => t.Vehicle)
				.FirstOrDefaultAsync(t => t.tripId == id);

			if (trip == null)
				return NotFound();

			await PopulateDropdowns();
			return View(trip);
		}


		// ================= POST =================
		[HttpPost]
		[ValidateAntiForgeryToken]
		[Authorize(Roles = "Admin,FleetManager")]
		public async Task<IActionResult> UpdateTripStatus(Trip trip)
		{
			try
			{
				var existingTrip = await _context.Trips
					.Include(t => t.Driver)
					.Include(t => t.Vehicle)
					.FirstOrDefaultAsync(t => t.tripId == trip.tripId);

				if (existingTrip == null)
					return NotFound();

				// ✅ Update basic fields
				existingTrip.origin = trip.origin;
				existingTrip.destination = trip.destination;
				existingTrip.plannedRoute = trip.plannedRoute;

				// ✅ Update status
				existingTrip.tripStatus = trip.tripStatus;

				// 🔥 MAIN LOGIC (IMPORTANT)

				// WHEN TRIP STARTS
				if (trip.tripStatus == TripStatus.IN_PROGRESS)
				{
					if (existingTrip.Driver != null)
						existingTrip.Driver.status = DriverStatus.ON_TRIP;

					if (existingTrip.Vehicle != null)
						existingTrip.Vehicle.status = VehicleStatus.ON_TRIP;
				}

				// WHEN TRIP COMPLETES
				if (trip.tripStatus == TripStatus.COMPLETED)
				{
					existingTrip.endDateTime = DateTime.Now;

					if (existingTrip.Driver != null)
						existingTrip.Driver.status = DriverStatus.AVAILABLE;

					if (existingTrip.Vehicle != null)
						existingTrip.Vehicle.status = VehicleStatus.ACTIVE;
				}

				// ✅ SAVE
				await _context.SaveChangesAsync();

				TempData["success"] = "Trip updated successfully!";
				return RedirectToAction("Index");
			}
			catch (Exception ex)
			{
				TempData["error"] = "Error updating trip: " + ex.Message;
				await PopulateDropdowns();
				return View(trip);
			}
		}


		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> StartTrip(int tripId)
		{
			try
			{
				// Get trip with driver & vehicle
				var trip = await _context.Trips
					.Include(t => t.Driver)
					.Include(t => t.Vehicle)
					.FirstOrDefaultAsync(t => t.tripId == tripId);

				if (trip == null)
					return NotFound();

				// Only allow planned trips
				if (trip.tripStatus != TripStatus.PLANNED)
				{
					TempData["Error"] = "Trip is not in planned state!";
					return RedirectToAction("Dashboard", "Driver");
				}

				// 🔥 FUEL VALIDATION
				double requiredFuel = 30;

				if (trip.Vehicle == null)
				{
					TempData["Error"] = "Vehicle not found!";
					return RedirectToAction("Dashboard", "Driver");
				}

				if (trip.Vehicle.currentfuel < requiredFuel)
				{
					TempData["Error"] = $"Minimum {requiredFuel}L fuel required before starting trip!";
					return RedirectToAction("AddFuelEntry", "Fuel");
				}

				// ✅ START TRIP
				trip.tripStatus = TripStatus.IN_PROGRESS;
				trip.startDateTime = DateTime.Now;
				trip.Fuelused = requiredFuel;
				// Update driver status
				if (trip.Driver != null)
					trip.Driver.status = DriverStatus.ON_TRIP;

				// Update vehicle status
				if (trip.Vehicle != null)
					trip.Vehicle.status = VehicleStatus.IN_SERVICE;

				await _context.SaveChangesAsync();

				TempData["Success"] = "Trip started successfully!";
				return RedirectToAction("Dashboard", "Driver");
			}
			catch (Exception ex)
			{
				TempData["Error"] = "Error starting trip: " + ex.Message;
				return RedirectToAction("Dashboard", "Driver");
			}
		}



		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> CompleteTrip(int tripId)
		{
			try
			{
				var trip = await _context.Trips
					.Include(t => t.Driver)
					.Include(t => t.Vehicle)
					.FirstOrDefaultAsync(t => t.tripId == tripId);

				if (trip == null)
					return NotFound();

				
				trip.tripStatus = TripStatus.COMPLETED;
				trip.endDateTime = DateTime.Now;

				
				if (trip.Driver != null)
					trip.Driver.status = DriverStatus.AVAILABLE;


				if (trip.Vehicle != null)
				{
					trip.Vehicle.status = VehicleStatus.ACTIVE;
					trip.Vehicle.currentfuel = 0;
				}
				await _context.SaveChangesAsync();

				TempData["success"] = "Trip completed successfully!";
				return RedirectToAction("Dashboard", "Driver");
			}
			catch (Exception ex)
			{
				TempData["error"] = ex.Message;
				return RedirectToAction("Dashboard", "Driver");
			}
		}


	}
}