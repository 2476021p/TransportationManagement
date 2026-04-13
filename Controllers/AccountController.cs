using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TransportationManagement.Data;
using TransportationManagement.Models;
using TransportationManagement.ViewModels;

namespace TransportationManagement.Controllers
{
	public class AccountController : Controller
	{
		private readonly SignInManager<ApplicationUser> _signInManager;
		private readonly UserManager<ApplicationUser> _userManager;
		private readonly ApplicationDbContext _context;

		public AccountController(
			SignInManager<ApplicationUser> signInManager,
			UserManager<ApplicationUser> userManager,
			ApplicationDbContext context)
		{
			_signInManager = signInManager;
			_userManager = userManager;
			_context = context;
		}

		
		[HttpGet]
		public IActionResult Login() => View();

	
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Login(LoginViewModel model)
		{
			if (!ModelState.IsValid) return View(model);

			var user = await _userManager.FindByEmailAsync(model.Email);
			if (user == null)
			{
				ModelState.AddModelError("",
					"Email not found in Identity system. Please create user first.");
				return View(model);
			}

			var result = await _signInManager.PasswordSignInAsync(
				user, model.Password, model.RememberMe, false);

			if (result.Succeeded)
			{
				var roles = await _userManager.GetRolesAsync(user);

				
				if (roles.Contains("Driver"))
				{
					var driver = await _context.Drivers
						.FirstOrDefaultAsync(d =>
							d.userId == user.Id ||
							d.Email.ToLower() == user.Email.ToLower());

					if (driver != null)
					{
						
						if (string.IsNullOrEmpty(driver.userId))
						{
							driver.userId = user.Id;
							_context.Update(driver);
							await _context.SaveChangesAsync();
						}

						HttpContext.Session.SetString("DriverId",
							driver.driverId.ToString());
						HttpContext.Session.SetString("Role", "Driver");
						return RedirectToAction("Dashboard", "Driver");
					}
					else
					{
						await _signInManager.SignOutAsync();
						ModelState.AddModelError("",
							"User found, but no matching record in Drivers table.");
						return View(model);
					}
				}

				if (roles.Contains("Admin"))
				{
					HttpContext.Session.SetString("Role", "Admin");
					return RedirectToAction("Dashboard", "Admin");
				}

				
				if (roles.Contains("FleetManager"))
				{
					HttpContext.Session.SetString("Role", "FleetManager");
					return RedirectToAction("Dashboard", "Admin");
				}

				if (roles.Contains("MaintenanceEngineer"))
				{
					HttpContext.Session.SetString("Role", "MaintenanceEngineer");
					return RedirectToAction("Index", "Maintenance");
				}


				return RedirectToAction("Index", "Home");
			}

			ModelState.AddModelError("", "Invalid password. Please try again.");
			return View(model);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> CreateUser(CreateUserViewModel model)
		{
			if (!ModelState.IsValid)
			{
				ViewBag.Roles = GetRolesList();
				return View(model);
			}

			var user = new ApplicationUser
			{
				UserName = model.Email,
				Email = model.Email,
				FullName = model.FullName
			};

			var result = await _userManager.CreateAsync(user, model.Password);

			if (result.Succeeded)
			{
				await _userManager.AddToRoleAsync(user, model.Role);

				if (model.Role == "Driver")
				{
					var existingDriver = await _context.Drivers
						.FirstOrDefaultAsync(d =>
							d.Email.ToLower() == model.Email.ToLower());

					if (existingDriver != null)
					{
						existingDriver.userId = user.Id;
						_context.Update(existingDriver);
					}
					else
					{
						var newDriver = new Driver
						{
							name = model.FullName,
							Email = model.Email,
							userId = user.Id,
							status = DriverStatus.AVAILABLE
						};
						_context.Drivers.Add(newDriver);
					}
					await _context.SaveChangesAsync();
				}

				TempData["Success"] = "Account created and linked successfully!";
				return RedirectToAction("Index", "Admin");
			}

			foreach (var error in result.Errors)
				ModelState.AddModelError("", error.Description);

			ViewBag.Roles = GetRolesList();
			return View(model);
		}

		[HttpPost]
		public async Task<IActionResult> Logout()
		{
			await _signInManager.SignOutAsync();
			HttpContext.Session.Clear();
			return RedirectToAction("Login");
		}

		private List<SelectListItem> GetRolesList()
		{
			return new List<SelectListItem>
			{
				new SelectListItem { Value = "Admin", Text = "Admin" },
				new SelectListItem { Value = "FleetManager", Text = "Fleet Manager" },
				new SelectListItem { Value = "Driver", Text = "Driver" },
             
                new SelectListItem { Value = "MaintenanceEngineer", Text = "Maintenance Engineer" }
			};
		}
	}
}
