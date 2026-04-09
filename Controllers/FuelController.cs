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

		public FuelController(FuelService fuelService, VehicleService vehicleService, ApplicationDbContext context)
		{
			_fuelService = fuelService;
			_vehicleService = vehicleService;
			_context = context;
		}

		// GET: Fuel
		public async Task<IActionResult> Index()
		{
			try
			{
				var entries = await _fuelService.GetAllFuelEntriesAsync();
				return View(entries);
			}
			catch (Exception ex)
			{
				TempData["Error"] = "An error occurred while loading fuel entries: " + ex.Message;
				return View(new List<FuelEntry>());
			}
		}

	     
		public async Task<IActionResult> AddFuelEntry()
		{
			try
			{
				await PopulateVehicles();
				return View();
			}
			catch (Exception ex)
			{
				TempData["Error"] = "An error occurred while loading the add fuel entry page: " + ex.Message;
				return RedirectToAction(nameof(Index));
			}
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> AddFuelEntry(FuelEntry fuelEntry)
		{
			try
			{
				if (!ModelState.IsValid)
				{
					await PopulateVehicles();
					return View(fuelEntry);
				}

				
				var vehicle = await _context.Vehicles
					.FirstOrDefaultAsync(v => v.vehicleId == fuelEntry.vehicleId);

				if (vehicle == null)
				{
					ModelState.AddModelError("", "Vehicle not found.");
					await PopulateVehicles();
					return View(fuelEntry);
				}

				
				_context.FuelEntries.Add(fuelEntry);

			
				vehicle.currentfuel += (double)fuelEntry.fuelQuantity;

				
				await _context.SaveChangesAsync();

				TempData["Success"] = "Fuel entry added successfully!";
				return RedirectToAction("Dashboard","Driver");
			}
			catch (Exception ex)
			{
				TempData["Error"] = "Error while adding fuel: " + ex.Message;
				await PopulateVehicles();
				return View(fuelEntry);
			}
		}


		public async Task<IActionResult> GetFuelConsumption(int vehicleId)
		{
			try
			{
				var entries = await _fuelService.GetFuelConsumptionAsync(vehicleId);
				ViewBag.VehicleId = vehicleId;
				return View(entries);
			}
			catch (Exception ex)
			{
				TempData["Error"] = "An error occurred while loading fuel consumption: " + ex.Message;
				return RedirectToAction(nameof(Index));
			}
		}

	
		public async Task<IActionResult> GenerateFuelReport()
		{
			try
			{
				var report = await _fuelService.GenerateFuelReportAsync();
				return View(report);
			}
			catch (Exception ex)
			{
				TempData["Error"] = "An error occurred while generating the fuel report: " + ex.Message;
				return RedirectToAction(nameof(Index));
			}
		}

		
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
				TempData["Error"] = "An error occurred while loading the fuel entry: " + ex.Message;
				return RedirectToAction(nameof(Index));
			}
		}

	
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(int id, FuelEntry fuelEntry)
		{
			try
			{
				if (id != fuelEntry.fuelId) return NotFound();
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
				TempData["Error"] = "An error occurred while updating the fuel entry: " + ex.Message;
				await PopulateVehicles();
				return View(fuelEntry);
			}
		}

		
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
				TempData["Error"] = "An error occurred while loading the delete page: " + ex.Message;
				return RedirectToAction(nameof(Index));
			}
		}

		
		[HttpPost, ActionName("Delete")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> DeleteConfirmed(int id)
		{
			try
			{
				await _fuelService.DeleteFuelEntryAsync(id);
				TempData["Success"] = "Fuel entry deleted.";
				return RedirectToAction(nameof(Index));
			}
			catch (Exception ex)
			{
				TempData["Error"] = "An error occurred while deleting the fuel entry: " + ex.Message;
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
			catch (Exception ex)
			{
				TempData["Error"] = "An error occurred while populating vehicles: " + ex.Message;
				ViewBag.Vehicles = new SelectList(new List<Vehicle>(), "vehicleId", "vehicleNumber");
			}
		}
	}
}