using System;
using yandex_pract.Services.BookingService.Modesl;

namespace yandex_pract.Services.BookingService;

public interface IBookingService
{
	Booking CreateBookingAsync(Guid eventId);
	(bool haveBooking, Booking result) GetBookingByIdAsync(Guid eventId);
}