using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using TransportationManagement.Data;
using TransportationManagement.Models;
using TransportationManagement.ViewModels;

namespace TransportationManagement.Controllers
{
	[Authorize(Roles = "Admin,FleetManager")]
	public class AdminController : Controller
	{
		private readonly ApplicationDbContext _context;
		private readonly UserManager<ApplicationUser> _userManager;

		public AdminController(
			ApplicationDbContext context,
			UserManager<ApplicationUser> userManager)
		{
			_context = context;
			_userManager = userManager;
		}


		public IActionResult Dashboard()
		{
			var model = new AdminDashboardViewModel
			{
				TotalVehicles = _context.Vehicles.Count(),
				TotalDrivers = _context.Drivers.Count(),
				TotalTrips = _context.Trips.Count(),
				TotalMaintenanceRecords = _context.MaintenanceRecords.Count(),
				TotalFuelEntries = _context.FuelEntries.Count(),
				TotalUsers = _userManager.Users.Count()
			};

			return View(model);
		}

		[Authorize(Roles = "Admin")]

		public async Task<IActionResult> Users()
		{
			var users = _userManager.Users.ToList();

			var model = new List<UserWithRoleViewModel>();

			foreach (var u in users)
			{
				
				var roles = await _userManager.GetRolesAsync(u);

				model.Add(new UserWithRoleViewModel
				{
					Id = u.Id,
					FullName = u.FullName ?? "",
					Email = u.Email ?? "",
					Role = roles.FirstOrDefault() ?? "No Role" 
				});
			}

			return View(model);
		}

		
		[HttpGet]
		public IActionResult CreateUser()
		{
			
			ViewBag.Roles = new List<SelectListItem>
	{
		new SelectListItem { Value = "Admin", Text = "Admin" },
		new SelectListItem { Value = "FleetManager", Text = "Fleet Manager" },
		//new SelectListItem { Value = "Driver", Text = "Driver" },
		new SelectListItem { Value = "MaintenanceEngineer", Text = "Maintenance Engineer" }

	};

			return View();
		}

	
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> CreateUser(CreateUserViewModel model)
		{
			if (!ModelState.IsValid)
			{
				
				ViewBag.Roles = new List<SelectListItem>
		{
			new SelectListItem { Value = "Admin", Text = "Admin" },
			new SelectListItem { Value = "FleetManager", Text = "Fleet Manager" },
			new SelectListItem { Value = "Driver", Text = "Driver" },
			new SelectListItem { Value = "MaintenanceEngineer", Text = "Maintenance Engineer" }
		};
				return View(model);
			}

			var user = new ApplicationUser
			{
				UserName = model.Email,
				Email = model.Email,
				FullName = model.FullName
			};

			var result = await _userManager.CreateAsync(user, model.Password);

			if (!result.Succeeded)
			{
				foreach (var error in result.Errors)
					ModelState.AddModelError("", error.Description);

				ViewBag.Roles = new List<SelectListItem>
		{
			new SelectListItem { Value = "Admin", Text = "Admin" },
			new SelectListItem { Value = "FleetManager", Text = "Fleet Manager" },
			new SelectListItem { Value = "Driver", Text = "Driver" },
			new SelectListItem { Value = "MaintenanceEngineer", Text = "Maintenance Engineer" }
		};
				return View(model);
			}

			
			await _userManager.AddToRoleAsync(user, model.Role);

			TempData["success"] = "User created successfully!";
			return RedirectToAction(nameof(Users));
		}

		
		[HttpGet]
		public async Task<IActionResult> EditUser(string id)
		{
			var user = await _userManager.FindByIdAsync(id);
			if (user == null)
				return NotFound();

			var roles = await _userManager.GetRolesAsync(user);

			var model = new EditUserViewModel
			{
				Id = user.Id,
				FullName = user.FullName,
				Email = user.Email ?? "",
				Role = roles.FirstOrDefault() ?? ""
			};

			ViewBag.Roles = new List<SelectListItem>
	{
		new SelectListItem { Value = "Admin", Text = "Admin" },
		new SelectListItem { Value = "FleetManager", Text = "Fleet Manager" },
		new SelectListItem { Value = "Driver", Text = "Driver" },
		new SelectListItem { Value = "MaintenanceEngineer", Text = "Maintenance Engineer" }
	};

			return View(model);
		}

		
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> EditUser(EditUserViewModel model)
		{
			if (!ModelState.IsValid)
				return View(model);

			var user = await _userManager.FindByIdAsync(model.Id);
			if (user == null)
				return NotFound();

			user.FullName = model.FullName;
			user.Email = model.Email;
			user.UserName = model.Email;

			await _userManager.UpdateAsync(user);

			
			var currentRoles = await _userManager.GetRolesAsync(user);
			await _userManager.RemoveFromRolesAsync(user, currentRoles);
			await _userManager.AddToRoleAsync(user, model.Role);

			TempData["success"] = "User updated successfully!";
			return RedirectToAction(nameof(Users));
		}

	
		public async Task<IActionResult> DeleteUser(string id)
		{
			var user = await _userManager.FindByIdAsync(id);
			if (user == null)
				return NotFound();

			await _userManager.DeleteAsync(user);
			TempData["success"] = "User deleted successfully!";
			return RedirectToAction(nameof(Users));
		}



		
		public IActionResult Drivers()
		{
			var drivers = _context.Drivers.ToList();
			return View(drivers);
		}

	
		[HttpGet]
		public IActionResult AddDriver()
		{
			return View("~/Views/Driver/AddDriver.cshtml");
		}

		[HttpPost]
		public async Task<IActionResult> AddDriver(Driver driver, string email, string password)
		{
			if (!ModelState.IsValid)
				return View("~/Views/Driver/AddDriver.cshtml", driver);

		
			_context.Drivers.Add(driver);
			await _context.SaveChangesAsync();

			
			var user = new ApplicationUser
			{
				UserName = email,
				Email = email,
				FullName = driver.name,
				driverId = driver.driverId
			};

			var result = await _userManager.CreateAsync(user, password);

			if (!result.Succeeded)
			{
				foreach (var error in result.Errors)
					ModelState.AddModelError("", error.Description);

				return View("~/Views/Driver/AddDriver.cshtml", driver);
			}

			await _userManager.AddToRoleAsync(user, "Driver");

			
			driver.userId = user.Id;
			_context.Update(driver);
			await _context.SaveChangesAsync();

			return RedirectToAction(nameof(Drivers));
		}

	
		[HttpGet]
		public async Task<IActionResult> EditDriver(int id)
		{
			var driver = await _context.Drivers.FindAsync(id);
			if (driver == null) return NotFound();

			return View("~/Views/Driver/AddDriver.cshtml", driver);
		}

		
		[HttpPost]
		public async Task<IActionResult> EditDriver(Driver driver)
		{
			if (!ModelState.IsValid)
				return View("~/Views/Driver/AddDriver.cshtml", driver);

			_context.Update(driver);
			await _context.SaveChangesAsync();

			return RedirectToAction(nameof(Drivers));
		}

		public async Task<IActionResult> DeleteDriver(int id)
		{
			var driver = await _context.Drivers.FindAsync(id);

			if (driver != null)
			{
				_context.Drivers.Remove(driver);
				await _context.SaveChangesAsync();
			}

			return RedirectToAction(nameof(Drivers));
		}


		public IActionResult Vehicles()
		{
			return RedirectToAction("Index", "Vehicle");
		}

		public IActionResult Trips()
		{
			return RedirectToAction("Index", "Trip");
		}

		public IActionResult Fuel()
		{
			return RedirectToAction("Index", "Fuel");
		}

		public IActionResult Maintenance()
		{
			return RedirectToAction("Index", "Maintenance");
		}
	}
}