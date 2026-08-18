using Application.Services.Abstraction.RequestResult;
using Application.Services.BookingService.Dto;
using Domain.Models.Users;

namespace Application.Services.Abstraction.Services;

public interface IBookingService
{
	Task<Result<BookingDto>> CreateBookingAsync(Guid eventId, Guid UserId);
	Task<Result<BookingDto>> GetBookingByIdAsync(Guid bookingId);
	Task<Result<bool>> CancelBookingAsync(Guid bookingId, Guid userId, UserRole role);
}