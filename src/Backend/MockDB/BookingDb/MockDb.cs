using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using yandex_pract.Services.BookingService.Models;

namespace yandex_pract.MockDB;

public partial class MockDb : IBookingDataBase
{
	private Dictionary<Guid, Booking> _bookings = new Dictionary<Guid, Booking>();
	private ConcurrentQueue<Booking> _awaitingBookings = new();

	public Booking Dequeue()
	{
		_awaitingBookings.TryDequeue(out var booking);
		return booking;
	}

	public void Enqueue(Booking booking)
	{
		_awaitingBookings.Enqueue(booking);
		_bookings.TryAdd(booking.Id, booking);
	}

	public bool TryFindBooking(Guid bookingId, out Booking result)
	{
		return _bookings.TryGetValue(bookingId, out result);
	}
}