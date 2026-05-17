using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using yandex_pract.Services.BookingService.Modesl;

namespace yandex_pract.MockDB;

public partial class MockDb : IBookingDataBase
{
	private Dictionary<Guid, Booking> _bookings = new Dictionary<Guid, Booking>();
	private ConcurrentQueue<Booking> _awaitingBookings = new();

	public (bool hasBooking, Booking? booking) TryDequeue()
	{
		var hasBooking = _awaitingBookings.TryDequeue(out var booking);
		return (hasBooking, booking);
	}

	public Booking Enqueue(Guid eventId)
	{
		var result = new Booking(eventId);
		_awaitingBookings.Enqueue(result);
		return result;
	}

	public bool TryFindBooking(Guid guid, out Booking result)
	{
		return _bookings.TryGetValue(guid, out result);
	}

	public bool TryAddBooking(Guid eventId, Booking booking)
	{
		return _bookings.TryAdd(eventId, booking);
	}
}