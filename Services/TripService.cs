using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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

		public async Task<IEnumerable<Trip>> GetAllTripsAsync()
		{
			return await _tripRepository.GetAllTripsAsync();
		}

		public async Task<Trip?> GetTripPlanAsync(int tripId)
		{
			return await _tripRepository.GetTripByIdAsync(tripId);
		}

		// CREATE TRIP WITH STATUS UPDATE
		public async Task CreateTripAsync(Trip trip)
		{
			// Check driver busy
			if (trip.driverId.HasValue)
			{
				if (await _tripRepository.IsDriverBusyAsync(
					trip.driverId.Value))
					throw new Exception(
						"Driver is already in an ongoing trip.");
			}

			// Check vehicle busy
			if (await _tripRepository.IsVehicleBusyAsync(trip.vehicleId))
				throw new Exception(
					"Vehicle is already in an ongoing trip.");

			// Set trip status
			trip.tripStatus = TripStatus.IN_PROGRESS;

			// Update Driver status
			if (trip.driverId.HasValue)
			{
				var driver = await _driverRepository
					.GetDriverByIdAsync(trip.driverId.Value);
				if (driver != null)
				{
					driver.status = DriverStatus.ON_TRIP;
					await _driverRepository.UpdateDriverAsync(driver);
				}
			}

			// Update Vehicle status
			var vehicle = await _vehicleRepository
				.GetVehicleByIdAsync(trip.vehicleId);
			if (vehicle != null)
			{
				vehicle.status = VehicleStatus.IN_SERVICE;
				await _vehicleRepository.UpdateVehicleAsync(vehicle);
			}

			// Save trip
			await _tripRepository.AddTripAsync(trip);
		}

		// UPDATE TRIP STATUS
		public async Task UpdateTripStatusAsync(Trip trip)
		{
			// IF IN PROGRESS - set busy
			if (trip.tripStatus == TripStatus.IN_PROGRESS)
			{
				if (trip.driverId.HasValue)
				{
					var driver = await _driverRepository
						.GetDriverByIdAsync(trip.driverId.Value);
					if (driver != null)
					{
						driver.status = DriverStatus.ON_TRIP;
						await _driverRepository.UpdateDriverAsync(driver);
					}
				}

				var vehicle = await _vehicleRepository
					.GetVehicleByIdAsync(trip.vehicleId);
				if (vehicle != null)
				{
					vehicle.status = VehicleStatus.IN_SERVICE;
					await _vehicleRepository.UpdateVehicleAsync(vehicle);
				}
			}

			// IF COMPLETED - free them
			if (trip.tripStatus == TripStatus.COMPLETED)
			{
				if (trip.driverId.HasValue)
				{
					var driver = await _driverRepository
						.GetDriverByIdAsync(trip.driverId.Value);
					if (driver != null)
					{
						driver.status = DriverStatus.AVAILABLE;
						await _driverRepository.UpdateDriverAsync(driver);
					}
				}

				var vehicle = await _vehicleRepository
					.GetVehicleByIdAsync(trip.vehicleId);
				if (vehicle != null)
				{
					vehicle.status = VehicleStatus.ACTIVE;
					await _vehicleRepository.UpdateVehicleAsync(vehicle);
				}
			}

			// Save trip update
			await _tripRepository.UpdateTripAsync(trip);
		}

		public async Task DeleteTripAsync(int tripId)
		{
			await _tripRepository.DeleteTripAsync(tripId);
		}

		public async Task<IEnumerable<Trip>> GetAssignedTripsAsync(
			int driverId)
		{
			return await _tripRepository
				.GetTripsByDriverIdAsync(driverId);
		}

		public async Task<bool> IsDriverBusyAsync(int driverId)
		{
			return await _tripRepository.IsDriverBusyAsync(driverId);
		}

		public async Task<bool> IsVehicleBusyAsync(int vehicleId)
		{
			return await _tripRepository.IsVehicleBusyAsync(vehicleId);
		}

		public async Task<IEnumerable<Trip>> GetTripsByDriverIdAsync(
			int driverId)
		{
			return await _tripRepository
				.GetTripsByDriverIdAsync(driverId);
		}

		public async Task<Trip?> GetTripByIdAsync(int tripId)
		{
			return await _tripRepository.GetTripByIdAsync(tripId);
		}

		public async Task UpdateTripAsync(Trip trip)
		{
			await _tripRepository.UpdateTripAsync(trip);
		}
	}
}