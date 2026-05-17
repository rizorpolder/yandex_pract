using System;
using yandex_pract.MockDB;
using yandex_pract.Services.BookingService.Modesl;

namespace yandex_pract.Services.BookingService;

public class BookingService(IBookingDataBase db) : IBookingService
{
	
	public Booking CreateBookingAsync(Guid eventId)
	{
		return db.Enqueue(eventId);
	}

	public (bool haveBooking,Booking result) GetBookingByIdAsync(Guid eventId)
	{
		var haveBooking =  db.TryFindBooking(eventId, out var booking);
		return (haveBooking,booking);
	}
}