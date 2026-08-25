using Application.Services.Abstraction.RequestResult;
using BookingService.Application.Services.Booking.Dto;
using Common.Models;

namespace BookingService.Application.Services.Abstraction.Services;

public interface IBookingService
{
	Task<Result<BookingDto>> CreateBookingAsync(Guid eventId, Guid userId);
	Task<Result<BookingDto>> GetBookingByIdAsync(Guid bookingId);
	Task<Result<bool>> CancelBookingAsync(Guid bookingId, Guid userId, UserRole role);

	Task ConfirmBookingAsync(Guid bookingId);
	Task RejectBookingAsync(Guid bookingId);
}