using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TransportationManagement.Data;
using TransportationManagement.Models;
using TransportationManagement.Services;

namespace TransportationManagement.Controllers
{
	[Authorize]
	public class DriverController : Controller
	{
		private readonly DriverService _driverService;
		private readonly ApplicationDbContext _context;

		public DriverController(DriverService driverService, ApplicationDbContext context)
		{
			_driverService = driverService;
			_context = context;
		}

	
		[Authorize(Roles = "Admin,FleetManager")]
		[HttpGet]
		public async Task<IActionResult> Index()
		{
			try
			{
				var drivers = await _driverService.ListAllDriversAsync();
				return View(drivers);
			}
			catch (Exception ex)
			{
				TempData["Error"] = "Error loading drivers: " + ex.Message;
				return View(new List<Driver>());
			}
		}

		// --- 2. DRIVER DETAILS (Admin/FleetManager) ---
		[Authorize(Roles = "Admin,FleetManager")]
		[HttpGet]
		public async Task<IActionResult> GetDriverDetails(int id)
		{
			try
			{
				var driver = await _driverService.GetDriverByIdAsync(id);
				if (driver == null) return NotFound();
				return View(driver);
			}
			catch (Exception ex)
			{
				TempData["Error"] = "Error retrieving driver details: " + ex.Message;
				return RedirectToAction(nameof(Index));
			}
		}

		// --- 3. GET ASSIGNED TRIPS (Driver self) ---
		[HttpGet]
		public async Task<IActionResult> GetAssignedTrips(int id)
		{
			try
			{
				var trips = await _driverService.GetDriverDashboardDataAsync(id);
				ViewBag.DriverId = id;
				return View(trips);
			}
			catch (Exception ex)
			{
				TempData["Error"] = "Error loading assigned trips: " + ex.Message;
				return RedirectToAction(nameof(Index));
			}
		}

		// --- 4. CREATE DRIVER (GET) ---
		[Authorize(Roles = "FleetManager")]
		[HttpGet]
		public IActionResult Create() => View();

		// --- 5. CREATE DRIVER (POST) ---
		[HttpPost]
		[ValidateAntiForgeryToken]
		[Authorize(Roles = "FleetManager")]
		public async Task<IActionResult> Create(Driver driver, string email, string password)
		{
			try
			{
				// Exclude navigation properties from validation
				ModelState.Remove("User");
				ModelState.Remove("UserId");
				ModelState.Remove("Trips");
				ModelState.Remove("MaintenanceRecords");
				ModelState.Remove("FuelEntries");

				if (!ModelState.IsValid) return View(driver);

				// FIX: Use CreateDriverAsync, not UpdateDriverAsync
				var result = await _driverService.CreateDriverAsync(driver, email, password);

				if (result.Success)
				{
					TempData["Success"] = result.Message;
					return RedirectToAction(nameof(Index));
				}
				else
				{
					TempData["Error"] = result.Message;
					return View(driver);
				}
			}
			catch (Exception ex)
			{
				TempData["Error"] = "Critical error during creation: " + ex.Message;
				return View(driver);
			}
		}

		// --- 6. DRIVER DASHBOARD (Self) ---
		[HttpGet]
		public async Task<IActionResult> Dashboard()
		{
			try
			{
				var driverIdStr = HttpContext.Session.GetString("DriverId");

				if (string.IsNullOrEmpty(driverIdStr))
				{
					return RedirectToAction("Login", "Account");
				}

				var trips = await _driverService.GetDriverDashboardDataAsync(
					int.Parse(driverIdStr));
				return View(trips);
			}
			catch (Exception ex)
			{
				TempData["Error"] = "Could not load dashboard: " + ex.Message;
				return RedirectToAction("Login", "Account");
			}
		}

		// --- 7. EDIT DRIVER (GET) ---
		[Authorize(Roles = "FleetManager")]
		[HttpGet]
		public async Task<IActionResult> Edit(int id)
		{
			var driver = await _driverService.GetDriverByIdAsync(id);
			if (driver == null) return NotFound();
			return View(driver);
		}

		// --- 7. EDIT DRIVER (POST) ---
		[HttpPost]
		[ValidateAntiForgeryToken]
		[Authorize(Roles = "FleetManager")]
		public async Task<IActionResult> Edit(int id, Driver driver)
		{
			if (id != driver.driverId) return NotFound();

			ModelState.Remove("User");
			ModelState.Remove("UserId");

			if (!ModelState.IsValid) return View(driver);

			var result = await _driverService.UpdateDriverAsync(driver);
			if (result.Success)
			{
				TempData["Success"] = result.Message;
				return RedirectToAction(nameof(Index));
			}

			TempData["Error"] = result.Message;
			return View(driver);
		}

		// --- 8. DELETE DRIVER ---
		[HttpPost]
		[Authorize(Roles = "FleetManager")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Delete(int id)
		{
			var result = await _driverService.DeleteDriverAsync(id);
			if (result.Success)
				TempData["Success"] = result.Message;
			else
				TempData["Error"] = result.Message;

			return RedirectToAction(nameof(Index));
		}

		// --- 9. START TRIP ---
		[HttpPost]
		[ValidateAntiForgeryToken]
		[Authorize(Roles = "Driver")]
		public async Task<IActionResult> StartTrip(Trip newTrip)
		{
			try
			{
				// 🔹 Get driver & vehicle
				var driver = await _context.Drivers.FindAsync(newTrip.driverId);
				var vehicle = await _context.Vehicles.FindAsync(newTrip.vehicleId);

				if (driver == null || vehicle == null)
					return NotFound();

				// 🔴 1. CHECK: Driver already has active trip
				var existingTrip = await _context.Trips
					.FirstOrDefaultAsync(t => t.driverId == newTrip.driverId
										  && t.tripStatus == TripStatus.IN_PROGRESS);

				if (existingTrip != null)
				{
					TempData["Error"] = "Driver already has an active trip!";
					return RedirectToAction("Dashboard");
				}

				// 🔴 2. CHECK: Driver status
				if (driver.status == DriverStatus.ON_TRIP)
				{
					TempData["Error"] = "Driver is already on another trip!";
					return RedirectToAction("Dashboard");
				}

				// 🔴 3. CHECK: Vehicle availability
				if (vehicle.status == VehicleStatus.ON_TRIP
					|| vehicle.status == VehicleStatus.IN_SERVICE)
				{
					TempData["Error"] = "Vehicle is not available!";
					return RedirectToAction("Dashboard");
				}

				// 🔴 4. CHECK: Fuel
				double requiredFuel = 20;

				if (vehicle.currentfuel < requiredFuel)
				{
					TempData["Error"] = "Minimum  fuel required to start the trip!";
					return RedirectToAction("AddFuelEntry", "Fuel",
						new { vehicleId = vehicle.vehicleId });
				}

				// ✅ 5. START TRIP
				driver.status = DriverStatus.ON_TRIP;
				vehicle.status = VehicleStatus.ON_TRIP;

				newTrip.tripStatus = TripStatus.IN_PROGRESS;
				newTrip.startDateTime = DateTime.Now;

				_context.Trips.Add(newTrip);
				await _context.SaveChangesAsync();

				TempData["Success"] = "Trip started successfully!";
				return RedirectToAction("Dashboard");
			}
			catch (Exception ex)
			{
				TempData["Error"] = "Error starting trip: " + ex.Message;
				return RedirectToAction("Dashboard");
			}
		}

	}
}
