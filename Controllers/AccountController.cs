using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
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

		// LOGIN GET
		[HttpGet]
		public IActionResult Login(string? returnUrl = null)
		{
			ViewBag.ReturnUrl = returnUrl;
			return View();
		}

		// LOGIN POST
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Login(
			LoginViewModel model, string? returnUrl = null)
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
					// Find driver by UserId
					var driver = await _context.Drivers
						.FirstOrDefaultAsync(d => d.UserId == user.Id);

					if (driver != null)
					{
						HttpContext.Session.SetString("DriverId",
							driver.driverId.ToString());
						HttpContext.Session.SetString("Role", "Driver");
						return RedirectToAction("Dashboard", "Driver");
					}
					else
					{
						// Driver record not linked yet — show error
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
					return RedirectToAction("Dashboard", "FleetManager");
				}

				// ==== MAINTENANCE ====
				if (await _userManager.IsInRoleAsync(user, "MaintenanceEngineer"))
				{
					HttpContext.Session.SetString("Role", "MaintenanceEngineer");
					return RedirectToAction("Index", "Maintenance");
				}
			}

			ModelState.AddModelError("", "Invalid login attempt");
			return View(model);
		}

		// LOGOUT
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Logout()
		{
			await _signInManager.SignOutAsync();
			HttpContext.Session.Clear();
			return RedirectToAction("Login", "Account");
		}

		// HELPER
		private IActionResult RedirectToLocal(string? returnUrl)
		{
			if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
				return Redirect(returnUrl);

			return RedirectToAction("Dashboard", "Admin");
		}
	}
}
