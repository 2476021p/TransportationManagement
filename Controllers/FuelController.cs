using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TransportationManagement.Data;
using TransportationManagement.Models;
using TransportationManagement.Services;

namespace TransportationManagement.Controllers
{
	[Authorize(Roles = "Admin,FleetManager,Driver")]
	public class FuelController : Controller
	{
		private readonly FuelService _fuelService;
		private readonly VehicleService _vehicleService;
		private readonly ApplicationDbContext _context;
		public FuelController(
			FuelService fuelService,
			VehicleService vehicleService, ApplicationDbContext context)
		{
			_fuelService = fuelService;
			_vehicleService = vehicleService;
			_context = context;
		}

		
		public async Task<IActionResult> Index()
		{
			try
			{
				var entries = await _fuelService.GetAllFuelEntriesAsync();
				return View(entries);
			}
			catch (Exception ex)
			{
				TempData["Error"] = "Error loading fuel entries: " + ex.Message;
				return View(new List<FuelEntry>());
			}
		}

	
		[HttpGet]
		[Authorize(Roles = "Driver,FleetManager")]
		public async Task<IActionResult> AddFuelEntry(int? vehicleId)
		{
			try
			{
				await PopulateVehicles();

				var model = new FuelEntry();

				if (vehicleId.HasValue)
				{
					model.vehicleId = vehicleId.Value;
				}

				return View(model);
			}
			catch (Exception ex)
			{
				TempData["Error"] = "Error loading fuel entry page: " + ex.Message;
				return RedirectToAction("Dashboard", "Driver");
			}
		}


		
		[HttpPost]
		[ValidateAntiForgeryToken]
		[Authorize(Roles = "Driver,FleetManager")]
		public async Task<IActionResult> AddFuelEntry(FuelEntry fuelEntry)
		{
			try
			{
				
				ModelState.Remove("Vehicle");

				if (!ModelState.IsValid)
				{
					await PopulateVehicles();
					return View(fuelEntry);
				}

				
				var vehicle = await _context.Vehicles.FindAsync(fuelEntry.vehicleId);

				if (vehicle == null)
				{
					TempData["Error"] = "Vehicle not found!";
					return RedirectToAction("Dashboard", "Driver");
				}

			
				await _fuelService.AddFuelEntryAsync(fuelEntry);

				vehicle.currentfuel += (double)fuelEntry.fuelQuantity;

				await _context.SaveChangesAsync();

				TempData["Success"] = "Fuel added successfully!";

				return RedirectToAction("Dashboard", "Driver");
			}
			catch (Exception ex)
			{
				TempData["Error"] = "Error adding fuel entry: " + ex.Message;

				await PopulateVehicles();
				return View(fuelEntry);
			}
		}


		[HttpGet]
		[Authorize(Roles = "Admin,FleetManager")]
		public async Task<IActionResult> GenerateFuelReport(int? vehicleId)
		{
			try
			{
				var report = await _fuelService.GenerateFuelReportAsync();
				if (vehicleId != null)
					report = report.Where(f => f.vehicleId == vehicleId).ToList();

				ViewBag.Vehicles = await _vehicleService.ListAllVehiclesAsync();
				return View(report);
			}
			catch (Exception ex)
			{
				TempData["Error"] = "Error generating report: " + ex.Message;
				return RedirectToAction(nameof(Index));
			}
		}

		[HttpGet]
		[Authorize(Roles = "FleetManager")]
		public async Task<IActionResult> Edit(int id)
		{
			try
			{
				var entry = await _fuelService.GetFuelEntryByIdAsync(id);
				if (entry == null) return NotFound();
				await PopulateVehicles();
				return View(entry);
			}
			catch (Exception ex)
			{
				TempData["Error"] = "Error loading fuel entry: " + ex.Message;
				return RedirectToAction(nameof(Index));
			}
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		[Authorize(Roles = "FleetManager")]
		public async Task<IActionResult> Edit(FuelEntry fuelEntry)
		{
			try
			{
				ModelState.Remove("Vehicle");

				if (ModelState.IsValid)
				{
					await _fuelService.UpdateFuelEntryAsync(fuelEntry);
					TempData["Success"] = "Fuel entry updated successfully.";
					return RedirectToAction(nameof(Index));
				}

				await PopulateVehicles();
				return View(fuelEntry);
			}
			catch (Exception ex)
			{
				TempData["Error"] = "Error updating fuel entry: " + ex.Message;
				await PopulateVehicles();
				return View(fuelEntry);
			}
		}

	
		[HttpGet]
		[Authorize(Roles = "FleetManager")]
		public async Task<IActionResult> Delete(int id)
		{
			try
			{
				var entry = await _fuelService.GetFuelEntryByIdAsync(id);
				if (entry == null) return NotFound();
				return View(entry);
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
				await _fuelService.DeleteFuelEntryAsync(id);
				TempData["Success"] = "Fuel entry deleted successfully.";
				return RedirectToAction(nameof(Index));
			}
			catch (Exception ex)
			{
				TempData["Error"] = "Error deleting fuel entry: " + ex.Message;
				return RedirectToAction(nameof(Index));
			}
		}

		private async Task PopulateVehicles()
		{
			try
			{
				var vehicles = await _vehicleService.ListAllVehiclesAsync();
				ViewBag.Vehicles = new SelectList(
					vehicles, "vehicleId", "vehicleNumber");
			}
			catch
			{
				ViewBag.Vehicles = new SelectList(
					Enumerable.Empty<SelectListItem>());
				throw;
			}
		}
	}
}
