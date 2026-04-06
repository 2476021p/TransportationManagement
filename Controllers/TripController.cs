using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using TransportationManagement.Models;
using TransportationManagement.Services;

namespace TransportationManagement.Controllers
{
	[Authorize(Roles = "Admin,FleetManager,Driver")]
	public class TripController : Controller
	{
		private readonly TripService _tripService;
		private readonly VehicleService _vehicleService;
		private readonly DriverService _driverService;

		public TripController(TripService tripService, VehicleService vehicleService, DriverService driverService)
		{
			_tripService = tripService;
			_vehicleService = vehicleService;
			_driverService = driverService;
		}

		// GET: Trip
		public async Task<IActionResult> Index()
		{
			var trips = await _tripService.GetAllTripsAsync();
			return View(trips);
		}

		// GET: Trip/Details/5
		public async Task<IActionResult> GetTripPlan(int id)
		{
			var trip = await _tripService.GetTripPlanAsync(id);
			if (trip == null) return NotFound();
			return View(trip);
		}

		// GET: Trip/Create
		[Authorize(Roles = "Admin,FleetManager")]
		public async Task<IActionResult> CreateTrip()
		{
			await PopulateDropdowns();
			return View();
		}

		// POST: Trip/Create
		[HttpPost]
		[ValidateAntiForgeryToken]
		[Authorize(Roles = "Admin,FleetManager")]
		public async Task<IActionResult> CreateTrip(Trip trip)
		{
			if (ModelState.IsValid)
			{
				// 🔥 STEP 1: Check Driver
				bool driverBusy = await _tripService.IsDriverBusyAsync(trip.driverId ?? 0);

				if (driverBusy)
				{
					TempData["Error"] = "Driver is already assigned to an ongoing trip.";
					await PopulateDropdowns();
					return View(trip);
				}

				// 🔥 STEP 2: Check Vehicle
				bool vehicleBusy = await _tripService.IsVehicleBusyAsync(trip.vehicleId);

				if (vehicleBusy)
				{
					TempData["Error"] = "Vehicle is already assigned to an ongoing trip.";
					await PopulateDropdowns();
					return View(trip);
				}

				var trips = await _tripService.GetAllTripsAsync();

				bool duplicateTrip = trips.Any(t =>
					t.driverId == trip.driverId &&
					t.vehicleId == trip.vehicleId &&
					t.origin.Trim().ToLower() == trip.origin.Trim().ToLower() &&
					t.destination.Trim().ToLower() == trip.destination.Trim().ToLower()
				);

				if (duplicateTrip)
				{
					TempData["Error"] = "Same trip already exists!";
					await PopulateDropdowns();
					return View(trip);
				}

				// ✅ STEP 4: SAVE
				await _tripService.CreateTripAsync(trip);

				TempData["Success"] = "Trip created successfully.";
				return RedirectToAction(nameof(Index));
			}

			await PopulateDropdowns();
			return View(trip);
		}
		// GET: Trip/UpdateTripStatus/5
		[Authorize(Roles = "Admin,FleetManager,Driver")]
        public async Task<IActionResult> UpdateTripStatus(int id)
        {
            var trip = await _tripService.GetTripPlanAsync(id);
            if (trip == null) return NotFound();
            await PopulateDropdowns();
            return View(trip);
        }

		// POST: Trip/UpdateTripStatus/5
		[HttpPost]
		[ValidateAntiForgeryToken]
		[Authorize(Roles = "Admin,FleetManager,Driver")]
		public async Task<IActionResult> UpdateTripStatus(int id, Trip trip)
		{
			if (id != trip.tripId)
				return NotFound();

		
			var existingTrip = await _tripService.GetTripPlanAsync(id);

			if (existingTrip == null)
				return NotFound();

			
			existingTrip.tripStatus = trip.tripStatus;

			
			await _tripService.UpdateTripStatusAsync(existingTrip);

			TempData["Success"] = "Trip status updated successfully.";
			return RedirectToAction(nameof(Index));
		}

		// GET: Trip/Delete/5
		[Authorize(Roles = "Admin,FleetManager")]
        public async Task<IActionResult> Delete(int id)
        {
            var trip = await _tripService.GetTripPlanAsync(id);
            if (trip == null) return NotFound();
            return View(trip);
        }

        // POST: Trip/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,FleetManager")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _tripService.DeleteTripAsync(id);
            TempData["Success"] = "Trip deleted successfully.";
            return RedirectToAction(nameof(Index));
        }

        private async Task PopulateDropdowns()
        {
            var vehicles = await _vehicleService.ListAllVehiclesAsync();
            var drivers = await _driverService.GetAllDriversAsync();
            ViewBag.Vehicles = new SelectList(vehicles, "vehicleId", "vehicleNumber");
            ViewBag.Drivers = new SelectList(drivers, "driverId", "name");
        }
    }
}
