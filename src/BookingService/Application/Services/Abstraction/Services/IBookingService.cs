using Application.Services.Abstraction.RequestResult;
using BookingService.Application.Services.Booking.Dto;

namespace BookingService.Application.Services.Abstraction.Services;

public interface IBookingService
{
	Task ApplyReservationResultAsync(Guid bookingId, bool success, string? failureReason);
	Task<Result<BookingDto>> CreateBookingAsync(Guid eventId, Guid UserId);
	Task<Result<BookingDto>> GetBookingByIdAsync(Guid bookingId);
	Task<Result<bool>> CancelBookingAsync(Guid bookingId, Guid userId);
}