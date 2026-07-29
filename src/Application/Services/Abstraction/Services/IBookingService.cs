using Domain.Models.Booking;

namespace Application.Services.Abstraction.Services;

public interface IBookingService
{
	Task<(bool result, Booking booking)> CreateBookingAsync(Guid eventId);
	Task<(bool haveBooking, Booking booking)> GetBookingByIdAsync(Guid bookingId);
}