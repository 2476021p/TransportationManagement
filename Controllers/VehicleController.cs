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

        // GET: Vehicle
        public async Task<IActionResult> Index()
        {
            var vehicles = await _vehicleService.ListAllVehiclesAsync();
            return View(vehicles);
        }

        // GET: Vehicle/Details/5
        public async Task<IActionResult> GetVehicleDetails(int id)
        {
            var vehicle = await _vehicleService.GetVehicleDetailsAsync(id);
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
                var vehicles = await _vehicleService.ListAllVehiclesAsync();

                if (vehicles.Any(v => v.vehicleNumber == vehicle.vehicleNumber))
                {
                    TempData["Error"] = "Cannot Create, Vehicle Number already exists!";
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
            var vehicle = await _vehicleService.GetVehicleDetailsAsync(id);
            if (vehicle == null) return NotFound();
            return View(vehicle);
        }

        // POST: Vehicle/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateVehicle(int id, Vehicle vehicle)
        {
            if (id != vehicle.vehicleId) return NotFound();
            if (ModelState.IsValid)
            {
                await _vehicleService.UpdateVehicleAsync(vehicle);
                TempData["Success"] = "Vehicle updated successfully.";
                return RedirectToAction(nameof(Index));
            }
            return View(vehicle);
        }

        // GET: Vehicle/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var vehicle = 
            await _vehicleService.GetVehicleDetailsAsync(id);
            if (vehicle == null) 
            return NotFound();

            bool isAssigned = await _vehicleService.IsVehicleAssignedAsync(id);

            if(isAssigned)
            {
                TempData["Error"] = "Vehicle is assigned to a trip.Cannot delete Vehicle.";
                return RedirectToAction("Index");
            }

            await _vehicleService.DeleteVehicleAsync(id);

            TempData["Success"] = "Vehicle is Deleted Successfully";

            return RedirectToAction("Index");

            
        }

        // POST: Vehicle/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _vehicleService.DeleteVehicleAsync(id);
            TempData["Success"] = "Vehicle deleted successfully.";
            return RedirectToAction(nameof(Index));
        }
    }
}
