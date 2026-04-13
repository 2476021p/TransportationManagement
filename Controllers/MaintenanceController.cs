using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using TransportationManagement.Models;
using TransportationManagement.Services;

namespace TransportationManagement.Controllers
{
	[Authorize(Roles = "Admin,MaintenanceEngineer,FleetManager")]
	public class MaintenanceController : Controller
	{
		private readonly MaintenanceService _maintenanceService;
		private readonly VehicleService _vehicleService;

		public MaintenanceController(
			MaintenanceService maintenanceService,
			VehicleService vehicleService)
		{
			_maintenanceService = maintenanceService;
			_vehicleService = vehicleService;
		}

	
		public async Task<IActionResult> Index()
		{
			try
			{
				var records = await _maintenanceService.GetAllMaintenanceRecordsAsync();
				return View(records);
			}
			catch (Exception ex)
			{
				TempData["Error"] = "Error loading maintenance records: " + ex.Message;
				return View(new List<MaintenanceRecord>());
			}
		}


		[HttpGet]
		public async Task<IActionResult> ScheduleMaintenance()
		{
			try
			{
				await PopulateVehicles();
				return View();
			}
			catch (Exception ex)
			{
				TempData["Error"] = "Error initializing schedule form: " + ex.Message;
				return RedirectToAction(nameof(Index));
			}
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> ScheduleMaintenance(MaintenanceRecord record)
		{
			try
			{
				ModelState.Remove("Vehicle");
				if (ModelState.IsValid)
				{
					var result = await _maintenanceService.ScheduleMaintenanceAsync(record);
					if (result.Success)
					{
						TempData["Success"] = result.Message;
						return RedirectToAction(nameof(Index));
					}
					TempData["Error"] = result.Message;
				}
				await PopulateVehicles();
				return View(record);
			}
			catch (Exception ex)
			{
				TempData["Error"] = "An error occurred while scheduling maintenance: "
					+ ex.Message;
				await PopulateVehicles();
				return View(record);
			}
		}
	
		[HttpGet]
		public async Task<IActionResult> UpdateServiceRecord(int id)
		{
			try
			{
				var record = await _maintenanceService.GetMaintenanceByIdAsync(id);
				if (record == null) return NotFound();

				if (record.status == MaintenanceStatus.COMPLETED)
				{
					TempData["Error"] = "Completed maintenance records cannot be modified.";
					return RedirectToAction(nameof(Index));
				}

				await PopulateVehicles();
				return View(record);
			}
			catch (Exception ex)
			{
				TempData["Error"] = "Error retrieving service record: " + ex.Message;
				return RedirectToAction(nameof(Index));
			}
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> UpdateServiceRecord(
			int id, MaintenanceRecord record)
		{
			try
			{
				ModelState.Remove("Vehicle");
				if (ModelState.IsValid)
				{
					var result = await _maintenanceService
						.UpdateServiceRecordAsync(id, record);
					if (result.Success)
					{
						TempData["Success"] = result.Message;
						return RedirectToAction(nameof(Index));
					}
					TempData["Error"] = result.Message;
				}
				await PopulateVehicles();
				return View(record);
			}
			catch (Exception ex)
			{
				TempData["Error"] = "An error occurred while updating the service record: "
					+ ex.Message;
				await PopulateVehicles();
				return View(record);
			}
		}

	
		[HttpGet]
		public async Task<IActionResult> Delete(int id)
		{
			try
			{
				var record = await _maintenanceService.GetMaintenanceByIdAsync(id);
				if (record == null) return NotFound();

				if (record.status == MaintenanceStatus.COMPLETED)
				{
					TempData["Error"] =
						"Completed maintenance records cannot be deleted for audit purposes.";
					return RedirectToAction(nameof(Index));
				}

				return View(record);
			}
			catch (Exception ex)
			{
				TempData["Error"] = "Error loading delete confirmation: " + ex.Message;
				return RedirectToAction(nameof(Index));
			}
		}

		
		[HttpPost, ActionName("Delete")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> DeleteConfirmed(int id)
		{
			try
			{
				var result = await _maintenanceService.DeleteMaintenanceAsync(id);
				if (result.Success)
					TempData["Success"] = result.Message;
				else
					TempData["Error"] = result.Message;

				return RedirectToAction(nameof(Index));
			}
			catch (Exception ex)
			{
				TempData["Error"] = "A system error occurred during deletion: " + ex.Message;
				return RedirectToAction(nameof(Index));
			}
		}

		[HttpGet]
		public async Task<IActionResult> GetMaintenanceHistory(int vehicleId)
		{
			try
			{
				var history = await _maintenanceService
					.GetMaintenanceHistoryAsync(vehicleId);
				ViewBag.VehicleId = vehicleId;
				return View(history);
			}
			catch (Exception ex)
			{
				TempData["Error"] = "Error retrieving maintenance history: " + ex.Message;
				return RedirectToAction(nameof(Index));
			}
		}

		
		private async Task PopulateVehicles()
		{
			try
			{
				var vehicles = await _vehicleService.ListAllVehiclesAsync();
				ViewBag.Vehicles = new SelectList(vehicles, "vehicleId", "vehicleNumber");
			}
			catch
			{
				ViewBag.Vehicles = new SelectList(Enumerable.Empty<SelectListItem>());
				throw;
			}
		}
	}
}
