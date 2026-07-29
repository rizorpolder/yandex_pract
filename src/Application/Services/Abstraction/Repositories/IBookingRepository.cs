using Domain.Models.Booking;

namespace Application.Services.Abstraction.Repositories;

public interface IBookingRepository
{
	Task<bool> EnqueueAsync(Booking booking);

	Task<(bool found, Booking booking)> TryFindBookingAsync(Guid bookingId);

	Task<List<Booking>> GetPendingAsync();

	Task UpdateBookingAsync(Booking booking);
}