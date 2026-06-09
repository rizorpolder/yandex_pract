using System;
using System.Collections.Generic;
using yandex_pract.Services.BookingService.Models;

namespace yandex_pract.MockDB;

public interface IBookingDataBase
{
	public void Enqueue(Booking booking);
	bool TryFindBooking(Guid bookingId, out Booking result);
	List<Booking> GetPending();

	void UpdateBooking(Booking booking);
}