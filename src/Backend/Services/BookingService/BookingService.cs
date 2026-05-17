using System;
using yandex_pract.MockDB;
using yandex_pract.Services.BookingService.Modesl;

namespace yandex_pract.Services.BookingService;

public class BookingService(IBookingDataBase db) : IBookingService
{
	
	public void CreateBookingAsync(Guid eventId)
	{
		
	}

	public Booking GetBookingByIdAsync(Guid eventId)
	{
		return null;
	}
}