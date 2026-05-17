using System;
using yandex_pract.Services.BookingService.Models;

namespace yandex_pract.MockDB;

public interface IBookingDataBase
{
	public Booking Dequeue();
	public void Enqueue(Booking booking);
	bool TryFindBooking(Guid bookingId, out Booking result);
}