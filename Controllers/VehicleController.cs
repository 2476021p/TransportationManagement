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
			var vehicles = await _vehicleService.ListAllVehiclesAsync();
			return View(vehicles);
		}

		// GET: Vehicle/Details/5
		public async Task<IActionResult> GetVehicleDetails(int id)
		{
			var vehicle = await _vehicleService
				.GetVehicleDetailsAsync(id);
			if (vehicle == null) return NotFound();
			return View(vehicle);
		}

		// GET: Vehicle/Create
		public IActionResult AddVehicle()
		{
			return View();
		}

		// POST: Vehicle/Create
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> AddVehicle(Vehicle vehicle)
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
						$"Vehicle number '{vehicle.vehicleNumber}'" +
						$" already exists!";
					return View(vehicle);
				}

				// Check duplicate registration number
				var existingReg = await _context.Vehicles
					.FirstOrDefaultAsync(v =>
						v.vehicleNumber ==
						vehicle.vehicleNumber);
				if (existingReg != null)
				{
					TempData["Error"] =
						$"Registration number " +
						$"'{vehicle.vehicleNumber}' " +
						$"already exists!";
					return View(vehicle);
				}

				await _vehicleService.AddVehicleAsync(vehicle);
				TempData["Success"] = "Vehicle added successfully.";
				return RedirectToAction(nameof(Index));
			}
			return View(vehicle);
		}

		// GET: Vehicle/Edit/5
		public async Task<IActionResult> UpdateVehicle(int id)
		{
			var vehicle = await _vehicleService
				.GetVehicleDetailsAsync(id);
			if (vehicle == null) return NotFound();
			return View(vehicle);
		}

		// POST: Vehicle/Edit/5
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> UpdateVehicle(
			int id, Vehicle vehicle)
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
						$"Vehicle number '{vehicle.vehicleNumber}'" +
						$" already exists!";
					return View(vehicle);
				}

				// Check duplicate registration excluding current
				var existingReg = await _context.Vehicles
					.FirstOrDefaultAsync(v =>
						v.vehicleNumber ==
						vehicle.vehicleNumber &&
						v.vehicleId != vehicle.vehicleId);
				if (existingReg != null)
				{
					TempData["Error"] =
						$"Registration number " +
						$"'{vehicle.vehicleNumber}' " +
						$"already exists!";
					return View(vehicle);
				}

				await _vehicleService.UpdateVehicleAsync(vehicle);
				TempData["Success"] = "Vehicle updated successfully.";
				return RedirectToAction(nameof(Index));
			}
			return View(vehicle);
		}

		// DELETE
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Delete(int id)
		{
			bool isAssigned = await _vehicleService
				.IsVehicleAssignedAsync(id);
			if (isAssigned)
			{
				TempData["Error"] =
					"Vehicle is on a trip. Cannot delete!";
				return RedirectToAction(nameof(Index));
			}

			await _vehicleService.DeleteVehicleAsync(id);
			TempData["Success"] = "Vehicle deleted successfully.";
			return RedirectToAction(nameof(Index));
		}
	}
}
