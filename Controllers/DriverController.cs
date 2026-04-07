using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
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
		private readonly TripService _tripService;
		private readonly UserManager<ApplicationUser> _userManager;
		private readonly ApplicationDbContext _context;

		public DriverController(
			DriverService driverService,
			TripService tripService,
			UserManager<ApplicationUser> userManager,
			ApplicationDbContext context)
		{
			_driverService = driverService;
			_tripService = tripService;
			_userManager = userManager;
			_context = context;
		}

		// ===================== INDEX =====================
		[Authorize(Roles = "Admin,FleetManager")]
		public async Task<IActionResult> Index()
		{
			var drivers = await _driverService.GetAllDriversAsync();
			return View(drivers);
		}

		// ===================== CREATE GET =====================
		[Authorize(Roles = "Admin,FleetManager")]
		public IActionResult Create()
		{
			return View();
		}

		// ===================== CREATE POST =====================
		[HttpPost]
		[ValidateAntiForgeryToken]
		[Authorize(Roles = "Admin,FleetManager")]
		public async Task<IActionResult> Create(Driver driver, string email, string password)
		{
			// Remove unwanted validations
			ModelState.Remove("User");
			ModelState.Remove("UserId");
			ModelState.Remove("Trips");

			// ❌ Model validation
			if (!ModelState.IsValid)
			{
				var modelErrors = string.Join(" | ", ModelState.Values
					.SelectMany(v => v.Errors)
					.Select(e => e.ErrorMessage));

				TempData["Error"] = "Failed: " + modelErrors;
				return View(driver);
			}

			// ❌ Email/password empty check
			if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
			{
				TempData["Error"] = "Email and Password are required.";
				return View(driver);
			}

			// ❌ Email already exists
			var existingUser = await _userManager.FindByEmailAsync(email);
			if (existingUser != null)
			{
				TempData["Error"] = "A user with this email already exists.";
				return View(driver);
			}

			// ✅ FIX 1: Duplicate contact number check
			var drivers = await _driverService.GetAllDriversAsync();

			if (drivers.Any(d => d.contactNumber == driver.contactNumber))
			{
				TempData["Error"] = "Contact number already exists!";
				return View(driver);
			}

			var numbers = await _driverService.GetAllDriversAsync();

			if (drivers.Any(d => d.licenseNumber == driver.licenseNumber))
			{
				TempData["Error"] = "license number already exists!";
				return View(driver);
			}

			// ✅ STEP 1 - Save driver
			driver.status = DriverStatus.AVAILABLE;
			await _driverService.AddDriverAsync(driver);

			// ✅ STEP 2 - Create Identity user
			var user = new ApplicationUser
			{
				UserName = email,
				Email = email
			};

			var result = await _userManager.CreateAsync(user, password);

			if (result.Succeeded)
			{
				// ✅ STEP 3 - Assign role
				await _userManager.AddToRoleAsync(user, "Driver");

				// ✅ STEP 4 - Link user to driver (your existing logic, just fixed safely)
				var savedDriver = await _context.Drivers
					.OrderByDescending(d => d.driverId)
					.FirstOrDefaultAsync();

				if (savedDriver != null)
				{
					savedDriver.userId = user.Id;
					await _driverService.UpdateDriverAsync(savedDriver);
				}

				TempData["Success"] = "Driver created successfully!";
				return RedirectToAction(nameof(Index));
			}
			else
			{
				// ❌ If user creation fails → rollback driver
				await _driverService.DeleteDriverAsync(driver.driverId);

				var identityErrors = string.Join(" | ",
					result.Errors.Select(e => e.Description));

				TempData["Error"] = "Account creation failed: " + identityErrors;
				return View(driver);
			}
		}


		// ===================== EDIT GET =====================
		[Authorize(Roles = "Admin,FleetManager")]
		public async Task<IActionResult> Edit(int id)
		{
			var driver = await _driverService.GetDriverByIdAsync(id);
			if (driver == null)
				return NotFound();
			return View(driver);
		}

		// ===================== EDIT POST =====================
		[Authorize(Roles = "Admin,FleetManager")]
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(Driver driver)
		{
			ModelState.Remove("User");
			ModelState.Remove("UserId");
			ModelState.Remove("Trips");

			if (!ModelState.IsValid)
				return View(driver);

			await _driverService.UpdateDriverAsync(driver);
			TempData["Success"] = "Driver updated successfully!";
			return RedirectToAction(nameof(Index));
		}

		// ===================== DELETE =====================
		// ===================== DELETE =====================
		[Authorize(Roles = "Admin,FleetManager")]
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Delete(int id)
		{
			var driver = await _driverService.GetDriverByIdAsync(id);
			if (driver == null)
			{
				TempData["Error"] = "Driver not found.";
				return RedirectToAction(nameof(Index));
			}

			// BLOCK if driver is ON a trip
			var activeTrip = await _context.Trips
				.FirstOrDefaultAsync(t => t.driverId == id &&
					t.tripStatus == TripStatus.IN_PROGRESS);

			if (activeTrip != null)
			{
				TempData["Error"] = $"Cannot delete '{driver.name}'. " +
					$"Driver is currently on a trip!";
				return RedirectToAction(nameof(Index));
			}

			// Delete Identity user account
			if (driver.userId != null)
			{
				var user = await _userManager.FindByIdAsync(driver.userId);
				if (user != null)
					await _userManager.DeleteAsync(user);
			}

			// Delete driver - SetNull in DbContext will automatically
			// set driverId to null in trips (trips are kept!)
			await _driverService.DeleteDriverAsync(id);
			TempData["Success"] = $"Driver '{driver.name}' deleted successfully!";
			return RedirectToAction(nameof(Index));
		}


		// ===================== DRIVER DASHBOARD =====================
		public async Task<IActionResult> Dashboard()
		{
			var driverIdString = HttpContext.Session.GetString("DriverId");

			if (string.IsNullOrEmpty(driverIdString))
				return RedirectToAction("Login", "Account");

			int driverId = int.Parse(driverIdString);
			var trips = await _tripService.GetTripsByDriverIdAsync(driverId);
			return View(trips);
		}

		// ===================== START TRIP =====================
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> StartTrip(int id)
		{
			var driverIdString = HttpContext.Session.GetString("DriverId");
			if (string.IsNullOrEmpty(driverIdString))
				return RedirectToAction("Login", "Account");

			var trip = await _context.Trips
				.Include(t => t.Vehicle)
				.Include(t => t.Driver)
				.FirstOrDefaultAsync(t => t.tripId == id);

			if (trip == null)
				return NotFound();

			// Update trip status
			trip.tripStatus = TripStatus.IN_PROGRESS;

			// Update driver status to ON_TRIP
			if (trip.Driver != null)
				trip.Driver.status = DriverStatus.ON_TRIP;

			// Update vehicle status to ON_TRIP
			if (trip.Vehicle != null)
				trip.Vehicle.status = VehicleStatus.IN_SERVICE;

			await _context.SaveChangesAsync();

			TempData["success"] = "🚗 Trip started! Drive safe!";
			return RedirectToAction("Dashboard");
		}

		// ===================== COMPLETE TRIP =====================
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> UpdateTripStatus(int id)
		{
			var driverIdString = HttpContext.Session.GetString("DriverId");
			if (string.IsNullOrEmpty(driverIdString))
				return RedirectToAction("Login", "Account");

			var trip = await _context.Trips
				.Include(t => t.Vehicle)
				.Include(t => t.Driver)
				.FirstOrDefaultAsync(t => t.tripId == id);

			if (trip == null)
				return NotFound();

			// Update trip to COMPLETED
			trip.tripStatus = TripStatus.COMPLETED;

			// Set driver back to AVAILABLE
			if (trip.Driver != null)
				trip.Driver.status = DriverStatus.AVAILABLE;

			// Set vehicle back to AVAILABLE
			if (trip.Vehicle != null)
				trip.Vehicle.status = VehicleStatus.ACTIVE;

			await _context.SaveChangesAsync();

			TempData["success"] = "Trip completed! ";
			return RedirectToAction("Dashboard");
		}
	}
}