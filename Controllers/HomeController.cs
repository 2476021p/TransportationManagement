using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace TransportationManagement.Controllers
{
	public class HomeController : Controller
	{
		[Authorize]
		public IActionResult Index()
		{
			try
			{
				if (User.IsInRole("Admin"))
					return RedirectToAction("Dashboard", "Admin");
				return View();
			}
			catch (Exception ex)
			{
				TempData["Error"] = "An error occurred: " + ex.Message;
				return View();
			}
		}

		public IActionResult Privacy()
		{
			try
			{
				return View();
			}
			catch (Exception ex)
			{
				TempData["Error"] = "An error occurred: " + ex.Message;
				return View();
			}
		}
	}
}
