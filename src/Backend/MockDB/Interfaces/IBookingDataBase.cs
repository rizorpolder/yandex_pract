using System;
using yandex_pract.Services.BookingService.Modesl;

namespace yandex_pract.MockDB;

public interface IBookingDataBase
{
	public (bool hasBooking, Booking? booking) TryDequeue();
	public Booking Enqueue(Guid eventId);
	bool TryFindBooking(Guid guid, out Booking result);
	public bool TryAddBooking(Guid eventId, Booking booking);
}