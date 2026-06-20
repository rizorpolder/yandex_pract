using System;
using System.Threading.Tasks;
using yandex_pract.CustomException;
using yandex_pract.MockDB;
using yandex_pract.Services.BookingService.Models;

namespace yandex_pract.Services.BookingService;

public class BookingService : IBookingService
{
	private readonly IBookingDataBase _bookingDataBase;
	private readonly object _bookingLock = new();
	private readonly IEventDataBase _eventDataBase;

	public BookingService(IBookingDataBase bookingDataBase, IEventDataBase eventDataBase)
	{
		_bookingDataBase = bookingDataBase;
		_eventDataBase = eventDataBase;
	}

	public Task<(bool result, Booking booking)> CreateBookingAsync(Guid eventId)
	{
		lock (_bookingLock)
		{
			var (hasEvent, eventData) = _eventDataBase.GetEventById(eventId);
			if (!hasEvent)
				return Task.FromResult((false, (Booking)null));

			if (!eventData.TryReserveSeats())
				throw new NoAvailableSeatsException();

			_eventDataBase.Update(eventData);

			var booking = new Booking(eventId);
			_bookingDataBase.Enqueue(booking);

			return Task.FromResult((true, booking));
		}
	}

	public Task<(bool haveBooking, Booking booking)> GetBookingByIdAsync(Guid bookingId)
	{
		var haveBooking = _bookingDataBase.TryFindBooking(bookingId, out var booking);
		return Task.FromResult((haveBooking, booking));
	}
}