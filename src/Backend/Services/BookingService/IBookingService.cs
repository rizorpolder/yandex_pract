using System;
using yandex_pract.Services.BookingService.Modesl;

namespace yandex_pract.Services.BookingService;

public interface IBookingService
{
	void CreateBookingAsync(Guid eventId);
	Booking GetBookingByIdAsync(Guid eventId);
}