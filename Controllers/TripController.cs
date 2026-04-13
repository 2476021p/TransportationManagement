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

		public TripController(
			TripService tripService,
			VehicleService vehicleService,
			DriverService driverService)
		{
			_tripService = tripService;
			_vehicleService = vehicleService;
			_driverService = driverService;
		}

		
		public async Task<IActionResult> Index()
		{
			try
			{
				var trips = await _tripService.GetAllTripsAsync();
				return View(trips);
			}
			catch (Exception ex)
			{
				TempData["Error"] = "Error loading trips: " + ex.Message;
				return View(new List<Trip>());
			}
		}

	
		[HttpGet]
		public async Task<IActionResult> GetTripPlan(int id)
		{
			try
			{
				var trip = await _tripService.GetTripPlanAsync(id);
				if (trip == null) return NotFound();
				return View("GetTripPlan", trip);
			}
			catch (Exception ex)
			{
				TempData["Error"] = "Error retrieving trip details: " + ex.Message;
				return RedirectToAction(nameof(Index));
			}
		}


		[HttpGet]
		[Authorize(Roles = "FleetManager")]
		public async Task<IActionResult> CreateTrip()
		{
			try
			{
				await PopulateDropdowns();
				return View();
			}
			catch (Exception ex)
			{
				TempData["Error"] = "Error loading configuration: " + ex.Message;
				return RedirectToAction(nameof(Index));
			}
		}

		
		[HttpPost]
		[ValidateAntiForgeryToken]
		[Authorize(Roles = "FleetManager")]
		public async Task<IActionResult> CreateTrip(Trip trip)
		{
			try
			{
				
				ModelState.Remove("Driver");
				ModelState.Remove("Vehicle");

				if (!ModelState.IsValid)
				{
					await PopulateDropdowns();
					return View(trip);
				}

				var result = await _tripService.CreateTripAsync(trip);
				if (result.Success)
				{
					TempData["Success"] = result.Message;
					return RedirectToAction(nameof(Index));
				}

				TempData["Error"] = result.Message;
				await PopulateDropdowns();
				return View(trip);
			}
			catch (Exception ex)
			{
				TempData["Error"] = "An unexpected error occurred while creating the trip: "
					+ ex.Message;
				await PopulateDropdowns();
				return View(trip);
			}
		}

		[HttpGet]
		[Authorize(Roles = "FleetManager")]
		public async Task<IActionResult> UpdateTripStatus(int id)
		{
			try
			{
				var trip = await _tripService.GetTripPlanAsync(id);
				if (trip == null) return NotFound();
				await PopulateDropdowns();
				return View(trip);
			}
			catch (Exception ex)
			{
				TempData["Error"] = "Error loading trip for update: " + ex.Message;
				return RedirectToAction(nameof(Index));
			}
		}

		
		[HttpPost]
		[ValidateAntiForgeryToken]
		[Authorize(Roles = "FleetManager")]
		public async Task<IActionResult> UpdateTripStatus(Trip trip)
		{
			try
			{
				ModelState.Remove("Driver");
				ModelState.Remove("Vehicle");

				if (!ModelState.IsValid)
				{
					await PopulateDropdowns();
					return View(trip);
				}

				await _tripService.UpdateTripStatusAsync(trip);
				TempData["Success"] = "Trip updated successfully!";
				return RedirectToAction(nameof(Index));
			}
			catch (Exception ex)
			{
				TempData["Error"] = "Error updating trip: " + ex.Message;
				await PopulateDropdowns();
				return View(trip);
			}
		}

		[HttpGet]
		[Authorize(Roles = "FleetManager")]
		public async Task<IActionResult> Delete(int id)
		{
			try
			{
				var trip = await _tripService.GetTripPlanAsync(id);
				if (trip == null) return NotFound();
				return View(trip);
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
				await _tripService.DeleteTripAsync(id);
				TempData["Success"] = "Trip deleted successfully!";
				return RedirectToAction(nameof(Index));
			}
			catch (Exception ex)
			{
				TempData["Error"] = "Error deleting trip: " + ex.Message;
				return RedirectToAction(nameof(Index));
			}
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> StartTrip(int tripId)
		{
			try
			{
				var result = await _tripService.StartTripAsync(tripId);
				if (result.Success)
					TempData["Success"] = result.Message;
				else
					TempData["Error"] = result.Message;

				return RedirectToAction("Dashboard", "Driver");
			}
			catch (Exception ex)
			{
				TempData["Error"] = "Critical error starting trip: " + ex.Message;
				return RedirectToAction("Dashboard", "Driver");
			}
		}


		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> CompleteTrip(int tripId)
		{
			try
			{
				await _tripService.CompleteTripAsync(tripId);
				TempData["Success"] = "Trip completed successfully!";
				return RedirectToAction("Dashboard", "Driver");
			}
			catch (Exception ex)
			{
				TempData["Error"] = "Critical error completing trip: " + ex.Message;
				return RedirectToAction("Dashboard", "Driver");
			}
		}

		private async Task PopulateDropdowns()
		{
			try
			{
				var allVehicles = await _vehicleService.ListAllVehiclesAsync();
				var activeVehicles = allVehicles
					.Where(v => v.status == VehicleStatus.ACTIVE);
				var drivers = await _driverService.ListAllDriversAsync();

				ViewBag.Vehicles = new SelectList(
					activeVehicles, "vehicleId", "vehicleNumber");
				ViewBag.Drivers = new SelectList(
					drivers, "driverId", "name");
			}
			catch
			{
				ViewBag.Vehicles = new SelectList(
					Enumerable.Empty<SelectListItem>());
				ViewBag.Drivers = new SelectList(
					Enumerable.Empty<SelectListItem>());
				throw;
			}
		}
	}
}
