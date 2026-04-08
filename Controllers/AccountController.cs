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
		public IActionResult Login(string? returnUrl = null)
		{
			try
			{
				ViewBag.ReturnUrl = returnUrl ?? returnUrl;
				return View();
			}
			catch (Exception ex)
			{
				TempData["Error"] = "An error occurred while loading the login page: " + ex.Message;
				return View();
			}
		}

		
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
		{
			try
			{
				if (!ModelState.IsValid)
					return View(model);

				var result = await _signInManager.PasswordSignInAsync(
					model.Email,
					model.Password,
					model.RememberMe,
					false);

				if (result.Succeeded)
				{
					var user = await _userManager.FindByEmailAsync(model.Email);

					// ==== DRIVER ====
					if (await _userManager.IsInRoleAsync(user, "Driver"))
					{
						// Find driver by userId
						var driver = await _context.Drivers
							.FirstOrDefaultAsync(d => d.userId == user.Id);

						// If userId not linked yet, find by most recent unlinked driver
						if (driver == null)
						{
							driver = await _context.Drivers
								.Where(d => d.userId == null || d.userId == "")
								.OrderByDescending(d => d.driverId)
								.FirstOrDefaultAsync();

							if (driver != null)
							{
								// Link now
								driver.userId = user.Id;
								_context.Drivers.Update(driver);
								await _context.SaveChangesAsync();
							}
						}

						if (driver != null)
						{
							HttpContext.Session.SetString("DriverId",
								driver.driverId.ToString());
							HttpContext.Session.SetString("Role", "Driver");
							return RedirectToAction("Dashboard", "Driver");
						}
						else
						{
							await _signInManager.SignOutAsync();
							ModelState.AddModelError("",
								"Driver profile not found. Contact admin.");
							return View(model);
						}
					}


					// ==== ADMIN ====
					if (await _userManager.IsInRoleAsync(user, "Admin"))
					{
						HttpContext.Session.SetString("Role", "Admin");
						return RedirectToAction("Dashboard", "Admin");
					}

					// ==== FLEET MANAGER ====
					if (await _userManager.IsInRoleAsync(user, "FleetManager"))
					{
						HttpContext.Session.SetString("Role", "FleetManager");
						return RedirectToAction("Dashboard", "Admin");
					}

					// ==== MAINTENANCE ====
					if (await _userManager.IsInRoleAsync(user, "MaintenanceEngineer"))
					{
						HttpContext.Session.SetString("Role", "MaintenanceEngineer");
						return RedirectToAction("Index", "Maintenance");
					}
				}

				ModelState.AddModelError("", "Invalid login attempt.");
				return View(model);
			}
			catch (Exception ex)
			{
				TempData["Error"] = "An error occurred during login: " + ex.Message;
				return View(model);
			}
		}

		// LOGOUT
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Logout()
		{
			try
			{
				await _signInManager.SignOutAsync();
				HttpContext.Session.Clear();
				return RedirectToAction("Login", "Account");
			}
			catch (Exception ex)
			{
				TempData["Error"] = "An error occurred during logout: " + ex.Message;
				return RedirectToAction("Login", "Account");
			}
		}

		// HELPER
		private IActionResult RedirectToLocal(string returnUrl)
		{
			try
			{
				if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
					return Redirect(returnUrl);
				return RedirectToAction("Dashboard", "Admin");
			}
			catch (Exception ex)
			{
				TempData["Error"] = "Redirect error: " + ex.Message;
				return RedirectToAction("Dashboard", "Admin");
			}
		}

		// CREATE USER GET
		[HttpGet]
		public IActionResult CreateUser()
		{
			try
			{
				// Pass roles to ViewBag
				ViewBag.Roles = new List<SelectListItem>
				{
					new SelectListItem { Value = "Admin", Text = "Admin" },
					new SelectListItem { Value = "FleetManager", Text = "Fleet Manager" },
					new SelectListItem { Value = "Driver", Text = "Driver" },
					new SelectListItem { Value = "MaintenanceEngineer", Text = "Maintenance Engineer" }
				};
				return View();
			}
			catch (Exception ex)
			{
				TempData["Error"] = "An error occurred while loading the create user page: " + ex.Message;
				return RedirectToAction("Users");
			}
		}

		// CREATE USER POST
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> CreateUser(CreateUserViewModel model)
		{
			try
			{
				if (!ModelState.IsValid)
				{
					// Re-pass roles on validation failure
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

				if (result.Succeeded)
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

				// Assign Role
				await _userManager.AddToRoleAsync(user, model.Role);
				TempData["Success"] = "User created successfully!";
				return RedirectToAction(nameof(User));
			}
			catch (Exception ex)
			{
				TempData["Error"] = "An error occurred while creating the user: " + ex.Message;
				return View(model);
			}
		}

		// EDIT USER GET
		[HttpGet]
		public async Task<IActionResult> EditUser(string id)
		{
			try
			{
				var user = await _userManager.FindByIdAsync(id);
				if (user == null) return NotFound();

				var roles = await _userManager.GetRolesAsync(user);

				var model = new EditUserViewModel
				{
					Id = user.Id,
					FullName = user.FullName ?? "",
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
			catch (Exception ex)
			{
				TempData["Error"] = "An error occurred while loading the edit user page: " + ex.Message;
				return RedirectToAction(nameof(User));
			}
		}

		// EDIT USER POST
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> EditUser(EditUserViewModel model)
		{
			try
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

				// Update Role
				var currentRoles = await _userManager.GetRolesAsync(user);
				await _userManager.RemoveFromRolesAsync(user, currentRoles);
				await _userManager.AddToRoleAsync(user, model.Role);

				TempData["Success"] = "User updated successfully!";
				return RedirectToAction(nameof(User));
			}
			catch (Exception ex)
			{
				TempData["Error"] = "An error occurred while updating the user: " + ex.Message;
				return View(model);
			}
		}

		// DELETE USER
		public async Task<IActionResult> DeleteUser(string id)
		{
			try
			{
				var user = await _userManager.FindByIdAsync(id);
				if (user == null)
					return NotFound();
				await _userManager.DeleteAsync(user);
				TempData["Success"] = "User deleted successfully!";
				return RedirectToAction(nameof(User));
			}
			catch (Exception ex)
			{
				TempData["Error"] = "An error occurred while deleting the user: " + ex.Message;
				return RedirectToAction(nameof(User));
			}
		}

		// DRIVERS LIST
		public IActionResult Drivers()
		{
			try
			{
				var drivers = _context.Drivers.ToList();
				return View(drivers);
			}
			catch (Exception ex)
			{
				TempData["Error"] = "An error occurred while loading drivers: " + ex.Message;
				return View(new List<Driver>());
			}
		}
	}
}