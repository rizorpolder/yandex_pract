using Application.Services.Abstraction.RequestResult;
using Application.Services.BookingService.Dto;

namespace Application.Services.Abstraction.Services;

public interface IBookingService
{
	Task<Result<BookingDto>> CreateBookingAsync(Guid eventId);
	Task<Result<BookingDto>> GetBookingByIdAsync(Guid bookingId);
}