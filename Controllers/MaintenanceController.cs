using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TransportationManagement.Data;
using TransportationManagement.Models;
using TransportationManagement.Services;

namespace TransportationManagement.Controllers
{
	[Authorize(Roles = "Admin,MaintenanceEngineer,FleetManager")]
	public class MaintenanceController : Controller
	{
		private readonly MaintenanceService _maintenanceService;
		private readonly VehicleService _vehicleService;
		private readonly ApplicationDbContext _context;

		public MaintenanceController(
			MaintenanceService maintenanceService,
			VehicleService vehicleService,
			ApplicationDbContext context)
		{
			_maintenanceService = maintenanceService;
			_vehicleService = vehicleService;
			_context = context;
		}

		// GET: Maintenance
		public async Task<IActionResult> Index()
		{
			try
			{
				var records = await _maintenanceService.GetAllMaintenanceRecordsAsync();
				return View(records);
			}
			catch (Exception ex)
			{
				TempData["Error"] = "An error occurred while loading maintenance records: " + ex.Message;
				return View(new List<MaintenanceRecord>());
			}
		}

		// GET: Maintenance/ScheduleMaintenance
		public async Task<IActionResult> ScheduleMaintenance()
		{
			try
			{
				await PopulateVehicles();
				return View();
			}
			catch (Exception ex)
			{
				TempData["Error"] = "An error occurred while loading the schedule maintenance page: " + ex.Message;
				return RedirectToAction(nameof(Index));
			}
		}

		// POST: Maintenance/ScheduleMaintenance
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> ScheduleMaintenance(MaintenanceRecord record)
		{
			try
			{
				ModelState.Remove("Vehicle");

				if (ModelState.IsValid)
				{
					// CHECK 1 - Vehicle on a trip?
					bool onTrip = await _context.Trips
						.AnyAsync(t =>
							t.vehicleId == record.vehicleId &&
							t.tripStatus == TripStatus.IN_PROGRESS);

					if (onTrip)
					{
						TempData["Error"] =
							"Cannot schedule maintenance. " +
							"Vehicle is currently on a trip!";
						await PopulateVehicles();
						return View(record);
					}

					// CHECK 2 - Already has scheduled maintenance?
					bool alreadyScheduled = await _context.MaintenanceRecords
						.AnyAsync(m =>
							m.vehicleId == record.vehicleId &&
							m.status == MaintenanceStatus.SCHEDULED);

					if (alreadyScheduled)
					{
						TempData["Error"] =
							"This vehicle already has a " +
							"scheduled maintenance!";
						await PopulateVehicles();
						return View(record);
					}

					// Save maintenance record
					await _maintenanceService.ScheduleMaintenanceAsync(record);

					// Update vehicle status to IN_SERVICE
					var vehicle = await _context.Vehicles
						.FindAsync(record.vehicleId);
					if (vehicle != null)
					{
						vehicle.status = VehicleStatus.IN_SERVICE;
						await _context.SaveChangesAsync();
					}

					TempData["Success"] = "Maintenance scheduled successfully!";
					return RedirectToAction(nameof(Index));
				}

				await PopulateVehicles();
				return View(record);
			}
			catch (Exception ex)
			{
				TempData["Error"] = "An error occurred while scheduling maintenance: " + ex.Message;
				await PopulateVehicles();
				return View(record);
			}
		}

		// GET: Maintenance/UpdateServiceRecord/5
		public async Task<IActionResult> UpdateServiceRecord(int id)
		{
			try
			{
				var record = await _maintenanceService.GetMaintenanceByIdAsync(id);
				if (record == null) return NotFound();
				await PopulateVehicles();
				return View(record);
			}
			catch (Exception ex)
			{
				TempData["Error"] = "An error occurred while loading the service record: " + ex.Message;
				return RedirectToAction(nameof(Index));
			}
		}

		// POST: Maintenance/UpdateServiceRecord/5
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> UpdateServiceRecord(int id, MaintenanceRecord record)
		{
			try
			{
				ModelState.Remove("Vehicle");

				if (id != record.maintenanceId) return NotFound();
				if (ModelState.IsValid)
				{
					await _maintenanceService.UpdateServiceRecordAsync(record);

					// If completed - set vehicle back to ACTIVE
					if (record.status == MaintenanceStatus.COMPLETED)
					{
						var vehicle = await _context.Vehicles
							.FindAsync(record.vehicleId);
						if (vehicle != null)
						{
							vehicle.status = VehicleStatus.ACTIVE;
							await _context.SaveChangesAsync();
						}
					}

					TempData["Success"] = "Service record updated successfully.";
					return RedirectToAction(nameof(Index));
				}

				await PopulateVehicles();
				return View(record);
			}
			catch (Exception ex)
			{
				TempData["Error"] = "An error occurred while updating the service record: " + ex.Message;
				await PopulateVehicles();
				return View(record);
			}
		}

		// GET: Maintenance/GetMaintenanceHistory/5
		public async Task<IActionResult> GetMaintenanceHistory(int vehicleId)
		{
			try
			{
				var records = await _maintenanceService
					.GetMaintenanceHistoryAsync(vehicleId);
				ViewBag.VehicleId = vehicleId;
				return View(records);
			}
			catch (Exception ex)
			{
				TempData["Error"] = "An error occurred while loading maintenance history: " + ex.Message;
				return RedirectToAction(nameof(Index));
			}
		}

		// GET: Maintenance/Delete/5
		public async Task<IActionResult> Delete(int id)
		{
			try
			{
				var record = await _maintenanceService.GetMaintenanceByIdAsync(id);
				if (record == null) return NotFound();
				return View(record);
			}
			catch (Exception ex)
			{
				TempData["Error"] = "An error occurred while loading the delete page: " + ex.Message;
				return RedirectToAction(nameof(Index));
			}
		}

		// POST: Maintenance/Delete/5
		[HttpPost, ActionName("Delete")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> DeleteConfirmed(int id)
		{
			try
			{
				var record = await _maintenanceService.GetMaintenanceByIdAsync(id);
				if (record != null)
				{
					var vehicle = await _context.Vehicles
						.FindAsync(record.vehicleId);
					if (vehicle != null)
					{
						vehicle.status = VehicleStatus.ACTIVE;
						await _context.SaveChangesAsync();
					}
				}

				await _maintenanceService.DeleteMaintenanceAsync(id);
				TempData["Success"] = "Maintenance record deleted.";
				return RedirectToAction(nameof(Index));
			}
			catch (Exception ex)
			{
				TempData["Error"] = "An error occurred while deleting the maintenance record: " + ex.Message;
				return RedirectToAction(nameof(Index));
			}
		}

		// KEY FIX - Show ALL vehicles in dropdown
		private async Task PopulateVehicles()
		{
			try
			{
				// Show ALL vehicles - no status filter
				// Validation handles blocked vehicles in POST
				var vehicles = await _context.Vehicles.ToListAsync();
				ViewBag.Vehicles = new SelectList(
					vehicles, "vehicleId", "vehicleNumber");
			}
			catch (Exception ex)
			{
				TempData["Error"] = "An error occurred while populating vehicles: " + ex.Message;
				ViewBag.Vehicles = new SelectList(new List<Vehicle>(), "vehicleId", "vehicleNumber");
			}
		}
	}
}
