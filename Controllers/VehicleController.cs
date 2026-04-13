using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TransportationManagement.Models;
using TransportationManagement.Services;

namespace TransportationManagement.Controllers
{
	[Authorize(Roles = "Admin,FleetManager")]
	public class VehicleController : Controller
	{
		private readonly VehicleService _vehicleService;

		public VehicleController(VehicleService vehicleService)
		{
			_vehicleService = vehicleService;
		}

		public async Task<IActionResult> Index()
		{
			try
			{
				var vehicles = await _vehicleService.ListAllVehiclesAsync();
				return View(vehicles);
			}
			catch (Exception ex)
			{
				TempData["Error"] = "Error loading vehicles: " + ex.Message;
				return View(new List<Vehicle>());
			}
		}

		public async Task<IActionResult> GetVehicleDetails(int id)
		{
			try
			{
				var vehicle = await _vehicleService.GetVehicleDetailsAsync(id);
				if (vehicle == null) return NotFound();

				return View("GetVehicleDetails", vehicle);
			}
			catch (Exception ex)
			{
				TempData["Error"] = "Error retrieving vehicle details: " + ex.Message;
				return RedirectToAction(nameof(Index));
			}
		}

		[HttpGet]
		public IActionResult AddVehicle()
		{
			return View();
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> AddVehicle(Vehicle vehicle)
		{
			try
			{
				if (!ModelState.IsValid) return View(vehicle);

				var result = await _vehicleService.AddVehicleAsync(vehicle);
				if (result.Success)
				{
					TempData["Success"] = result.Message;
					return RedirectToAction(nameof(Index));
				}

				TempData["Error"] = result.Message;
				return View(vehicle);
			}
			catch (Exception ex)
			{
				TempData["Error"] = "An error occurred while adding the vehicle: " + ex.Message;
				return View(vehicle);
			}
		}

		public async Task<IActionResult> UpdateVehicle(int id)
		{
			try
			{
				var vehicle = await _vehicleService.GetVehicleDetailsAsync(id);
				if (vehicle == null) return NotFound();
				return View(vehicle);
			}
			catch (Exception ex)
			{
				TempData["Error"] = "Error loading vehicle for update: " + ex.Message;
				return RedirectToAction(nameof(Index));
			}
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> UpdateVehicle(int id, Vehicle vehicle)
		{
			try
			{
				if (id != vehicle.vehicleId) return NotFound();
				if (!ModelState.IsValid) return View(vehicle);

				var result = await _vehicleService.UpdateVehicleAsync(vehicle);
				if (result.Success)
				{
					TempData["Success"] = result.Message;
					return RedirectToAction(nameof(Index));
				}

				TempData["Error"] = result.Message;
				return View(vehicle);
			}
			catch (Exception ex)
			{
				TempData["Error"] = "Error updating vehicle: " + ex.Message;
				return View(vehicle);
			}
		}

		public async Task<IActionResult> Delete(int id)
		{
			try
			{
				var vehicle = await _vehicleService.GetVehicleDetailsAsync(id);
				if (vehicle == null) return NotFound();
				return View(vehicle);
			}
			catch (Exception ex)
			{
				TempData["Error"] = "Error loading delete confirmation: " + ex.Message;
				return RedirectToAction(nameof(Index));
			}
		}

		[HttpPost, ActionName("Delete")]
		[ValidateAntiForgeryToken]
		[Authorize(Roles = "FleetManager")]
		public async Task<IActionResult> DeleteConfirmed(int id)
		{
			try
			{
				var result = await _vehicleService.DeleteVehicleAsync(id);
				if (!result.Success)
					TempData["Error"] = result.Message;
				else
					TempData["Success"] = result.Message;

				return RedirectToAction(nameof(Index));
			}
			catch (Exception ex)
			{
				TempData["Error"] = "A system error occurred during deletion: " + ex.Message;
				return RedirectToAction(nameof(Index));
			}
		}

		[HttpPost]
		public async Task<IActionResult> UpdateStatus(int id, VehicleStatus status)
		{
			try
			{
				await _vehicleService.UpdateStatusAsync(id, status);
				TempData["Success"] = "Vehicle status updated successfully.";
				return RedirectToAction("Index");
			}
			catch (Exception ex)
			{
				TempData["Error"] = "Could not update status: " + ex.Message;
				return RedirectToAction("Index");
			}
		}
	}
}