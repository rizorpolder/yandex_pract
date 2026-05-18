using System;
using System.Threading.Tasks;
using yandex_pract.Services.BookingService.Models;

namespace yandex_pract.Services.BookingService;

public interface IBookingService
{
	Task<(bool result, Booking booking)> CreateBookingAsync(Guid eventId);
	Task<(bool haveBooking, Booking booking)> GetBookingByIdAsync(Guid bookingId);
}