using Domain.Models.Bookings;

namespace Application.Services.Abstraction.Repositories;

public interface IBookingRepository
{
	public Task AddBookingAsync(Booking booking);
	public Task DeleteBookingAsync(Booking booking);
	public Task<Booking?> GetBookingAsync(Guid bookingId);
	public Task SaveChangesAsync();
	public Task<List<Booking>> GetPendingAsync();
	public Task<int> GetActiveBookingsCountAsync(Guid userId);
}