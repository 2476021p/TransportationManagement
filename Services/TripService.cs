using TransportationManagement.Interfaces;
using TransportationManagement.Models;

namespace TransportationManagement.Services
{
	public class TripService
	{
		private readonly ITripRepository _tripRepository;
		private readonly IDriverRepository _driverRepository;
		private readonly IVehicleRepository _vehicleRepository;

		public TripService(
			ITripRepository tripRepository,
			IDriverRepository driverRepository,
			IVehicleRepository vehicleRepository)
		{
			_tripRepository = tripRepository;
			_driverRepository = driverRepository;
			_vehicleRepository = vehicleRepository;
		}

		public async Task<IEnumerable<Trip>> GetAllTripsAsync() =>
			await _tripRepository.GetAllTripsAsync();

		public async Task<Trip?> GetTripPlanAsync(int tripId) =>
			await _tripRepository.GetTripByIdAsync(tripId);

		public async Task<(bool Success, string Message)> CreateTripAsync(Trip trip)
		{
			// Check driver availability
			if (trip.driverId.HasValue &&
				await _tripRepository.IsDriverBusyAsync(trip.driverId.Value))
				return (false, "Driver is already assigned to another active trip.");

			// Check vehicle availability
			if (await _tripRepository.IsVehicleBusyAsync(trip.vehicleId))
				return (false, "Vehicle is already assigned to another active trip.");

			// Check vehicle status
			var vehicle = await _vehicleRepository.GetVehicleByIdAsync(trip.vehicleId);
			if (vehicle == null || vehicle.status == VehicleStatus.RETIRED)
				return (false, "Vehicle is not available for trips.");

			trip.tripStatus = TripStatus.PLANNED;
			trip.startDateTime = DateTime.Now;

			await _tripRepository.AddTripAsync(trip);

			// Update statuses immediately upon planning
			if (trip.driverId.HasValue)
			{
				var driver = await _driverRepository.GetDriverByIdAsync(trip.driverId.Value);
				if (driver != null) driver.status = DriverStatus.ON_TRIP;
			}

			if (vehicle != null) vehicle.status = VehicleStatus.ON_TRIP;

			await _tripRepository.SaveChangesAsync();
			return (true, "Trip created successfully!");
		}

		public async Task UpdateTripStatusAsync(Trip tripUpdate)
		{
			var existing = await _tripRepository.GetTripByIdAsync(tripUpdate.tripId);
			if (existing == null) return;

			existing.origin = tripUpdate.origin;
			existing.destination = tripUpdate.destination;
			existing.tripStatus = tripUpdate.tripStatus;

			if (existing.tripStatus == TripStatus.COMPLETED)
			{
				existing.endDateTime = DateTime.Now;
				if (existing.Driver != null)
					existing.Driver.status = DriverStatus.AVAILABLE;
				if (existing.Vehicle != null)
					existing.Vehicle.status = VehicleStatus.ACTIVE;
			}
			else if (existing.tripStatus == TripStatus.IN_PROGRESS)
			{
				if (existing.Driver != null)
					existing.Driver.status = DriverStatus.ON_TRIP;
				if (existing.Vehicle != null)
					existing.Vehicle.status = VehicleStatus.ON_TRIP;
			}

			await _tripRepository.UpdateTripAsync(existing);
		}

		public async Task<(bool Success, string Message)> StartTripAsync(int tripId)
		{
			var trip = await _tripRepository.GetTripByIdAsync(tripId);
			if (trip == null) return (false, "Trip not found.");
			if (trip.tripStatus != TripStatus.PLANNED)
				return (false, "Trip is not in PLANNED state.");

			// Check minimum fuel
			double requiredFuel = 20;
			if (trip.Vehicle == null || trip.Vehicle.currentfuel < requiredFuel)
				return (false, $"Minimum {requiredFuel}L fuel required!");

			trip.tripStatus = TripStatus.IN_PROGRESS;
			trip.startDateTime = DateTime.Now;
			trip.Fuelused = requiredFuel;

			if (trip.Driver != null) trip.Driver.status = DriverStatus.ON_TRIP;
			if (trip.Vehicle != null) trip.Vehicle.status = VehicleStatus.IN_SERVICE;

			await _tripRepository.UpdateTripAsync(trip);
			return (true, "Trip started successfully!");
		}

		public async Task CompleteTripAsync(int tripId)
		{
			var trip = await _tripRepository.GetTripByIdAsync(tripId);
			if (trip == null) return;

			trip.tripStatus = TripStatus.COMPLETED;
			trip.endDateTime = DateTime.Now;

			if (trip.Driver != null)
				trip.Driver.status = DriverStatus.AVAILABLE;

			if (trip.Vehicle != null)
			{
				trip.Vehicle.status = VehicleStatus.ACTIVE;
				trip.Vehicle.currentfuel = 0;
			}

			await _tripRepository.UpdateTripAsync(trip);
		}

		public async Task DeleteTripAsync(int id) =>
			await _tripRepository.DeleteTripAsync(id);
	}
}
