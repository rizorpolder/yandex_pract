using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using yandex_pract.Services.BookingService.Models;

namespace yandex_pract.DbContext.Interfaces;

public interface IBookingDataBase
{
	Task EnqueueAsync(Booking booking);

	Task<(bool found, Booking booking)> TryFindBookingAsync(Guid bookingId);

	Task<List<Booking>> GetPendingAsync();

	Task UpdateBookingAsync(Booking booking);
}