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

        public MaintenanceController(MaintenanceService maintenanceService, VehicleService vehicleService)
        {
            _maintenanceService = maintenanceService;
            _vehicleService = vehicleService;
        }

        // GET: Maintenance
        public async Task<IActionResult> Index()
        {
            var records = await _maintenanceService.GetAllMaintenanceRecordsAsync();
            return View(records);
        }

        // GET: Maintenance/ScheduleMaintenance
        public async Task<IActionResult> ScheduleMaintenance()
        {
            await PopulateVehicles();
            return View();
        }

        // POST: Maintenance/ScheduleMaintenance
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ScheduleMaintenance(MaintenanceRecord record)
        {
            if (ModelState.IsValid)
            {
                await _maintenanceService.ScheduleMaintenanceAsync(record);
                TempData["Success"] = "Maintenance scheduled successfully.";
                return RedirectToAction(nameof(Index));
            }
            await PopulateVehicles();
            return View(record);
        }

        // GET: Maintenance/UpdateServiceRecord/5
        public async Task<IActionResult> UpdateServiceRecord(int id)
        {
            var record = await _maintenanceService.GetMaintenanceByIdAsync(id);
            if (record == null) return NotFound();
            await PopulateVehicles();
            return View(record);
        }

        // POST: Maintenance/UpdateServiceRecord/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateServiceRecord(int id, MaintenanceRecord record)
        {
            if (id != record.maintenanceId) return NotFound();
            if (ModelState.IsValid)
            {
                await _maintenanceService.UpdateServiceRecordAsync(record);
                TempData["Success"] = "Service record updated successfully.";
                return RedirectToAction(nameof(Index));
            }
            await PopulateVehicles();
            return View(record);
        }

        // GET: Maintenance/GetMaintenanceHistory/5
        public async Task<IActionResult> GetMaintenanceHistory(int vehicleId)
        {
            var records = await _maintenanceService.GetMaintenanceHistoryAsync(vehicleId);
            ViewBag.VehicleId = vehicleId;
            return View(records);
        }

        // GET: Maintenance/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var record = await _maintenanceService.GetMaintenanceByIdAsync(id);
            if (record == null) return NotFound();
            return View(record);
        }

        // POST: Maintenance/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _maintenanceService.DeleteMaintenanceAsync(id);
            TempData["Success"] = "Maintenance record deleted.";
            return RedirectToAction(nameof(Index));
        }

        private async Task PopulateVehicles()
        {
            var vehicles = await _vehicleService.ListAllVehiclesAsync();
            ViewBag.Vehicles = new SelectList(vehicles, "vehicleId", "vehicleNumber");
        }
    }
}
