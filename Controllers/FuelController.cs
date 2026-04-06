using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using TransportationManagement.Models;
using TransportationManagement.Services;

namespace TransportationManagement.Controllers
{
    [Authorize(Roles = "Admin,FleetManager")]
    public class FuelController : Controller
    {
        private readonly FuelService _fuelService;
        private readonly VehicleService _vehicleService;

        public FuelController(FuelService fuelService, VehicleService vehicleService)
        {
            _fuelService = fuelService;
            _vehicleService = vehicleService;
        }

        // GET: Fuel
        public async Task<IActionResult> Index()
        {
            var entries = await _fuelService.GetAllFuelEntriesAsync();
            return View(entries);
        }

        // GET: Fuel/AddFuelEntry
        public async Task<IActionResult> AddFuelEntry()
        {
            await PopulateVehicles();
            return View();
        }

        // POST: Fuel/AddFuelEntry
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddFuelEntry(FuelEntry fuelEntry)
        {
            if (ModelState.IsValid)
            {
                await _fuelService.AddFuelEntryAsync(fuelEntry);
                TempData["Success"] = "Fuel entry added successfully.";
                return RedirectToAction(nameof(Index));
            }
            await PopulateVehicles();
            return View(fuelEntry);
        }

        // GET: Fuel/GetFuelConsumption/5
        public async Task<IActionResult> GetFuelConsumption(int vehicleId)
        {
            var entries = await _fuelService.GetFuelConsumptionAsync(vehicleId);
            ViewBag.VehicleId = vehicleId;
            return View(entries);
        }

        // GET: Fuel/GenerateFuelReport
        public async Task<IActionResult> GenerateFuelReport()
        {
            var report = await _fuelService.GenerateFuelReportAsync();
            return View(report);
        }

        // GET: Fuel/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var entry = await _fuelService.GetFuelEntryByIdAsync(id);
            if (entry == null) return NotFound();
            await PopulateVehicles();
            return View(entry);
        }

        // POST: Fuel/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, FuelEntry fuelEntry)
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

        // GET: Fuel/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var entry = await _fuelService.GetFuelEntryByIdAsync(id);
            if (entry == null) return NotFound();
            return View(entry);
        }

        // POST: Fuel/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _fuelService.DeleteFuelEntryAsync(id);
            TempData["Success"] = "Fuel entry deleted.";
            return RedirectToAction(nameof(Index));
        }

        private async Task PopulateVehicles()
        {
            var vehicles = await _vehicleService.ListAllVehiclesAsync();
            ViewBag.Vehicles = new SelectList(vehicles, "vehicleId", "vehicleNumber");
        }
    }
}
