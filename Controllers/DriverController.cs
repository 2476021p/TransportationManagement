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

		
		[Authorize(Roles = "FleetManager")]
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

		
		[HttpPost]
		[ValidateAntiForgeryToken]
		[Authorize(Roles = "FleetManager")]
		public async Task<IActionResult> Create(Driver driver, string email, string password)
		{
			try
			{
				ModelState.Remove("User");
				ModelState.Remove("UserId");
				ModelState.Remove("Trips");

				
				if (!ModelState.IsValid)
				{
					var modelErrors = string.Join(" | ", ModelState.Values
						.SelectMany(v => v.Errors)
						.Select(e => e.ErrorMessage));
					TempData["Error"] = "Failed: " + modelErrors;
					return View(driver);
				}

				
				if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
				{
					TempData["Error"] = "Email and Password are required.";
					return View(driver);
				}

			
				var existingUser = await _userManager.FindByEmailAsync(email);
				if (existingUser != null)
				{
					TempData["Error"] = "A user with this email already exists.";
					return View(driver);
				}

				
				var drivers = await _driverService.GetAllDriversAsync();
				if (drivers.Any(d => d.contactNumber == driver.contactNumber))
				{
					TempData["Error"] = "Contact number already exists!";
					return View(driver);
				}


				if (drivers.Any(d => d.licenseNumber == driver.licenseNumber))
				{
					TempData["Error"] = "License number already exists!";
					return View(driver);
				}

			
				driver.status = DriverStatus.AVAILABLE;
				await _driverService.AddDriverAsync(driver);

				var user = new ApplicationUser
				{
					UserName = email,
					Email = email
				};

				var result = await _userManager.CreateAsync(user, password);

				if (result.Succeeded)
				{
					
					await _userManager.AddToRoleAsync(user, "Driver");

					
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

		[Authorize(Roles = "FleetManager")]
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

		[HttpPost]
		[Authorize(Roles = "FleetManager")]
		public async Task<IActionResult> Edit(Driver driver)
		{
			try
			{
				if (!ModelState.IsValid)
					return View(driver);

				_context.Update(driver);
				await _context.SaveChangesAsync();
				return RedirectToAction(nameof(Index));
			}
			catch (Exception ex)
			{
				TempData["Error"] = "An error occurred while updating the driver: " + ex.Message;
				return View(driver);
			}
		}

		
		[HttpPost]
		[Authorize(Roles = "FleetManager")]
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

			
				var activeTrip = await _context.Trips
					.FirstOrDefaultAsync(t => t.driverId == id &&
											  t.tripStatus == TripStatus.IN_PROGRESS);
				if (activeTrip != null)
				{
					TempData["Error"] = $"Cannot delete '{driver.name}' - Driver is currently on a trip!";
					return RedirectToAction(nameof(Index));
				}

				
				if (driver.userId != null)
				{
					var user = await _userManager.FindByIdAsync(driver.userId);
					if (user != null)
						await _userManager.DeleteAsync(user);
				}

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

				
				trip.tripStatus = TripStatus.IN_PROGRESS;

				
				trip.startDateTime = DateTime.Now;

				
				if (trip.Driver != null)
					trip.Driver.status = DriverStatus.ON_TRIP;

				
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

		[HttpGet]
		[Authorize(Roles = "FleetManager")]
		public async Task<IActionResult> GetAssignedTrips(int id)
		{
			try
			{
				var trips = await _context.Trips
					.Include(t => t.Vehicle)
					.Include(t => t.Driver)
					.Where(t => t.driverId == id)
					.ToListAsync();

				return View(trips);
			}
			catch (Exception ex)
			{
				TempData["Error"] = "Error loading assigned trips: " + ex.Message;
				return RedirectToAction(nameof(Index));
			}
		}

		[HttpGet]
		[Authorize(Roles = "Admin,FleetManager")]
		public async Task<IActionResult> GetDriverDetails(int id)
		{
			var driver = await _context.Drivers.FirstOrDefaultAsync(d => d.driverId == id);

			if (driver == null)
				return NotFound();

			return View(driver);
		}


	}
}
