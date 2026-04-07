using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TransportationManagement.Data;
using TransportationManagement.Migrations;
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

		// ==================== INDEX ====================
		[Authorize(Roles = "Admin,FleetManager")]
		public async Task<IActionResult> Index()
		{
			try
			{
				var drivers = await _driverService.GetAllDriversAsync();
				return View(drivers);
			}
			catch (Exception ex)
			{
				TempData["Error"] = "An error occurred while loading drivers: " + ex.Message;
				return View(new List<Driver>());
			}
		}

		// ==================== CREATE GET ====================
		[Authorize(Roles = "Admin,FleetManager")]
		public IActionResult Create()
		{
			try
			{
				return View();
			}
			catch (Exception ex)
			{
				TempData["Error"] = "An error occurred while loading the create page: " + ex.Message;
				return RedirectToAction(nameof(Index));
			}
		}

		// ==================== CREATE POST ====================
		[HttpPost]
		[ValidateAntiForgeryToken]
		[Authorize(Roles = "Admin,FleetManager")]
		public async Task<IActionResult> Create(Driver driver, string email, string password)
		{
			try
			{
				// Remove unwanted validations
				ModelState.Remove("User");
				ModelState.Remove("UserId");
				ModelState.Remove("Trips");

				// X Model validation
				if (!ModelState.IsValid)
				{
					var modelErrors = string.Join(" | ", ModelState.Values
						.SelectMany(v => v.Errors)
						.Select(e => e.ErrorMessage));
					TempData["Error"] = "Failed: " + modelErrors;
					return View(driver);
				}

				// Email/password empty check
				if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
				{
					TempData["Error"] = "Email and Password are required.";
					return View(driver);
				}

				// X Email already exists
				var existingUser = await _userManager.FindByEmailAsync(email);
				if (existingUser != null)
				{
					TempData["Error"] = "A user with this email already exists.";
					return View(driver);
				}

				// Duplicate contact number check
				var drivers = await _driverService.GetAllDriversAsync();
				if (drivers.Any(d => d.contactNumber == driver.contactNumber))
				{
					TempData["Error"] = "Contact number already exists!";
					return View(driver);
				}

				// Duplicate license number check
				if (drivers.Any(d => d.licenseNumber == driver.licenseNumber))
				{
					TempData["Error"] = "License number already exists!";
					return View(driver);
				}

				// STEP 1 - Save driver
				driver.status = DriverStatus.AVAILABLE;
				await _driverService.AddDriverAsync(driver);

				// STEP 2 - Create Identity user
				var user = new ApplicationUser
				{
					UserName = email,
					Email = email
				};

				var result = await _userManager.CreateAsync(user, password);

				if (result.Succeeded)
				{
					// STEP 3 - Assign role
					await _userManager.AddToRoleAsync(user, "Driver");

					// STEP 4 - Link user to driver
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
					// X If user creation fails -> rollback driver
					await _driverService.DeleteDriverAsync(driver.driverId);
					var identityErrors = string.Join(" | ", result.Errors.Select(e => e.Description));
					TempData["Error"] = "Account creation failed: " + identityErrors;
					return View(driver);
				}
			}
			catch (Exception ex)
			{
				TempData["Error"] = "An unexpected error occurred while creating the driver: " + ex.Message;
				return View(driver);
			}
		}

		// ==================== EDIT GET ====================
		[Authorize(Roles = "Admin,FleetManager")]
		public async Task<IActionResult> Edit(int id)
		{
			try
			{
				var driver = await _driverService.GetDriverByIdAsync(id);
				if (driver == null) return NotFound();
				return View(driver);
			}
			catch (Exception ex)
			{
				TempData["Error"] = "An error occurred while loading the edit page: " + ex.Message;
				return RedirectToAction(nameof(Index));
			}
		}

		// ==================== EDIT POST ====================
		[HttpPost]
		[Authorize(Roles = "Admin,FleetManager")]
		public async Task<IActionResult> Edit(Driver driver)
		{
			try
			{
				if (!ModelState.IsValid)
					return View("~/Views/Driver/AddDriver.cshtml", driver);

				_context.Update(driver);
				await _context.SaveChangesAsync();
				return RedirectToAction(nameof(Index));
			}
			catch (Exception ex)
			{
				TempData["Error"] = "An error occurred while updating the driver: " + ex.Message;
				return View("~/Views/Driver/AddDriver.cshtml", driver);
			}
		}

		// ==================== DELETE DRIVER ====================
		[HttpPost]
		[Authorize(Roles = "Admin,FleetManager")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Delete(int id)
		{
			try
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
					TempData["Error"] = $"Cannot delete '{driver.name}' - Driver is currently on a trip!";
					return RedirectToAction(nameof(Index));
				}

				// Delete Identity user account
				if (driver.userId != null)
				{
					var user = await _userManager.FindByIdAsync(driver.userId);
					if (user != null)
						await _userManager.DeleteAsync(user);
				}

				// Delete driver
				await _driverService.DeleteDriverAsync(id);
				TempData["Success"] = $"Driver '{driver.name}' deleted successfully!";
				return RedirectToAction(nameof(Index));
			}
			catch (Exception ex)
			{
				TempData["Error"] = "An error occurred while deleting the driver: " + ex.Message;
				return RedirectToAction(nameof(Index));
			}
		}

		// ==================== DRIVER DASHBOARD ====================
		public async Task<IActionResult> Dashboard()
		{
			try
			{
				var driverIdString = HttpContext.Session.GetString("DriverId");
				if (string.IsNullOrEmpty(driverIdString))
					return RedirectToAction("Login", "Account");

				int driverId = int.Parse(driverIdString);

				var trips = await _context.Trips
					.Include(t => t.Vehicle)
					.Where(t => t.driverId == driverId)
					.ToListAsync();

				return View(trips);
			}
			catch (Exception ex)
			{
				TempData["Error"] = "An error occurred while loading the dashboard: " + ex.Message;
				return View(new List<Trip>());
			}
		}

		// ==================== START TRIP ====================
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> StartTrip(int id)
		{
			try
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

				// ✅ Save start date and time
				trip.startDateTime = DateTime.Now;

				// Update driver status to ON_TRIP
				if (trip.Driver != null)
					trip.Driver.status = DriverStatus.ON_TRIP;

				// Update vehicle status to IN_SERVICE
				if(trip.Vehicle != null)

					trip.Vehicle.status = VehicleStatus.IN_SERVICE;

				await _context.SaveChangesAsync();

				TempData["success"] = "Trip started! Drive safe!";
				return RedirectToAction("Dashboard");
			}
			catch (Exception ex)
			{
				TempData["Error"] = "An error occurred while starting the trip: " + ex.Message;
				return RedirectToAction("Dashboard");
			}
		}

		// ==================== COMPLETE TRIP ====================
		
	}
}
