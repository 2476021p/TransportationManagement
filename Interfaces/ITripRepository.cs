using TransportationManagement.Models;

namespace TransportationManagement.Interfaces
{
	public interface ITripRepository
	{
		Task<IEnumerable<Trip>> GetAllTripsAsync();
		Task<Trip?> GetTripByIdAsync(int tripId);
		Task<IEnumerable<Trip>> GetTripsByDriverIdAsync(int driverId);
		Task AddTripAsync(Trip trip);
		Task UpdateTripAsync(Trip trip);
		Task DeleteTripAsync(int tripId);
		Task<bool> TripExistsAsync(int tripId);
		Task<bool> IsDriverBusyAsync(int driverId);
		Task<bool> IsVehicleBusyAsync(int vehicleId);
		Task SaveChangesAsync();
	}
}
