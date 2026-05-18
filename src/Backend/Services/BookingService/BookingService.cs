using System;
using System.Threading.Tasks;
using yandex_pract.MockDB;
using yandex_pract.Services.BookingService.Models;

namespace yandex_pract.Services.BookingService;

public class BookingService(IBookingDataBase bookingDataBase, IEventDataBase eventDataBase) : IBookingService
{
	public Task<(bool result, Booking booking)> CreateBookingAsync(Guid eventId)
	{
		var (hasEvent, eventData) = eventDataBase.GetEventById(eventId);
		if (!hasEvent)
			return Task.FromResult((false, (Booking)null));

		var booking = new Booking(eventId);

		bookingDataBase.Enqueue(booking);

		return Task.FromResult((true, booking));
	}

	public Task<(bool haveBooking, Booking booking)> GetBookingByIdAsync(Guid bookingId)
	{
		var haveBooking = bookingDataBase.TryFindBooking(bookingId, out var booking);
		return Task.FromResult((haveBooking, booking));
	}
}