using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TransportationManagement.Data;
using TransportationManagement.Models;
using TransportationManagement.Services;

namespace TransportationManagement.Controllers
{
	[Authorize(Roles = "Admin,FleetManager")]
	public class VehicleController : Controller
	{
		private readonly VehicleService _vehicleService;
		private readonly ApplicationDbContext _context;

		public VehicleController(
			VehicleService vehicleService,
			ApplicationDbContext context)
		{
			_vehicleService = vehicleService;
			_context = context;
		}

		// GET: Vehicle
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

		// GET: Vehicle/Details/5
		public async Task<IActionResult> Details(int id)
		{
			try
			{
				var vehicle = await _vehicleService.GetVehicleDetailsAsync(id);
				if (vehicle == null) return NotFound();
				return View(vehicle);
			}
			catch (Exception ex)
			{
				TempData["Error"] = "Error loading vehicle details: " + ex.Message;
				return RedirectToAction(nameof(Index));
			}
		}

		// GET: Vehicle/Create
		public IActionResult Create()
		{
			try
			{
				return View();
			}
			catch (Exception ex)
			{
				TempData["Error"] = "Error loading create page: " + ex.Message;
				return RedirectToAction(nameof(Index));
			}
		}

		// POST: Vehicle/Create
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> AddVehicle(Vehicle vehicle)
		{
			try
			{
				if (ModelState.IsValid)
				{
					// Check duplicate vehicle number
					var existingVehicle = await _context.Vehicles
						.FirstOrDefaultAsync(v =>
							v.vehicleNumber == vehicle.vehicleNumber);
					if (existingVehicle != null)
					{
						TempData["Error"] =
							$"Vehicle number '{vehicle.vehicleNumber}' already exists!";
						return View(vehicle);
					}

					// Check duplicate registration number
					var existingReg = await _context.Vehicles
						.FirstOrDefaultAsync(v =>
							v.vehicleNumber == vehicle.vehicleNumber);
					if (existingReg != null)
					{
						TempData["Error"] =
							$"Registration '{vehicle.vehicleNumber}' already exists!";
						return View(vehicle);
					}

					await _vehicleService.AddVehicleAsync(vehicle);
					TempData["Success"] = "Vehicle added successfully.";
					return RedirectToAction(nameof(Index));
				}
				return View(vehicle);
			}
			catch (Exception ex)
			{
				TempData["Error"] = "Error adding vehicle: " + ex.Message;
				return View(vehicle);
			}
		}

		// GET: Vehicle/Edit/5
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
				TempData["Error"] = "Error loading edit page: " + ex.Message;
				return RedirectToAction(nameof(Index));
			}
		}

		// POST: Vehicle/Edit/5
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> UpdateVehicle(int id, Vehicle vehicle)
		{
			try
			{
				if (id != vehicle.vehicleId) return NotFound();

				if (ModelState.IsValid)
				{
					// Check duplicate vehicle number excluding current
					var existing = await _context.Vehicles
						.FirstOrDefaultAsync(v =>
							v.vehicleNumber == vehicle.vehicleNumber &&
							v.vehicleId != vehicle.vehicleId);
					if (existing != null)
					{
						TempData["Error"] =
							$"Vehicle number '{vehicle.vehicleNumber}' already exists!";
						return View(vehicle);
					}

					// Check duplicate registration excluding current
					var existingReg = await _context.Vehicles
						.FirstOrDefaultAsync(v =>
							v.vehicleNumber == vehicle.vehicleNumber &&
							v.vehicleId != vehicle.vehicleId);
					if (existingReg != null)
					{
						TempData["Error"] =
							$"Registration '{vehicle.vehicleNumber}' already exists!";
						return View(vehicle);
					}

					await _vehicleService.UpdateVehicleAsync(vehicle);
					TempData["Success"] = "Vehicle updated successfully.";
					return RedirectToAction(nameof(Index));
				}
				return View(vehicle);
			}
			catch (Exception ex)
			{
				TempData["Error"] = "Error updating vehicle: " + ex.Message;
				return View(vehicle);
			}
		}

		// ✅ GET: Vehicle/Delete/5 - Confirmation page
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
				TempData["Error"] = "Error loading delete page: " + ex.Message;
				return RedirectToAction(nameof(Index));
			}
		}

		// ✅ POST: Vehicle/Delete/5 - Actual delete
		[HttpPost, ActionName("Delete")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> DeleteConfirmed(int id)
		{
			try
			{
				bool isAssigned = await _vehicleService.IsVehicleAssignedAsync(id);
				if (isAssigned)
				{
					TempData["Error"] = "Vehicle is on a trip. Cannot delete!";
					return RedirectToAction(nameof(Index));
				}

				// Also check active maintenance
				var activeMaintenance = await _context.MaintenanceRecords
					.AnyAsync(m => m.vehicleId == id &&
								   m.status == MaintenanceStatus.SCHEDULED);
				if (activeMaintenance)
				{
					TempData["Error"] =
						"Vehicle has scheduled maintenance. Cannot delete!";
					return RedirectToAction(nameof(Index));
				}

				await _vehicleService.DeleteVehicleAsync(id);
				TempData["Success"] = "Vehicle deleted successfully.";
				return RedirectToAction(nameof(Index));
			}
			catch (Exception ex)
			{
				TempData["Error"] = "Error deleting vehicle: " + ex.Message;
				return RedirectToAction(nameof(Index));
			}
		}
	}
}
