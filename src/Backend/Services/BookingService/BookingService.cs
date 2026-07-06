using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using yandex_pract.CustomException;
using yandex_pract.DbContext;
using yandex_pract.MockDB;
using yandex_pract.Services.BookingService.Models;

namespace yandex_pract.Services.BookingService;

public class BookingService : IBookingService
{
	private readonly AppDbContext _dbContext;
	private readonly IEventDataBase _eventDataBase;
	private readonly SemaphoreSlim _semaphore = new(1, 1);

	public BookingService(AppDbContext dbContext, IEventDataBase eventDataBase)
	{
		_dbContext = dbContext;
		_eventDataBase = eventDataBase;
	}

	public async Task<(bool result, Booking booking)> CreateBookingAsync(Guid eventId)
	{
		await _semaphore.WaitAsync();
		try
		{
			var (hasEvent, eventData) = _eventDataBase.GetEventById(eventId);
			if (!hasEvent)
				return (false, null);

			if (!eventData.TryReserveSeats())
				throw new NoAvailableSeatsException("No available seats");

			_eventDataBase.Update(eventData);

			var booking = new Booking(eventId);

			_dbContext.Bookings.Add(booking);
			await _dbContext.SaveChangesAsync();

			return (true, booking);
		}
		finally
		{
			_semaphore.Release();
		}
	}

	public async Task<(bool haveBooking, Booking booking)> GetBookingByIdAsync(Guid bookingId)
	{
		var booking = await _dbContext.Bookings
			.FirstOrDefaultAsync(b => b.Id == bookingId);

		return (booking != null, booking);
	}

	public async Task<List<Booking>> DequeuePendingAsync()
	{
		await _semaphore.WaitAsync();
		try
		{
			var pending = await _dbContext.Bookings
				.Where(b => b.Status == BookingStatus.Pending)
				.ToListAsync();

			foreach (var booking in pending)
			{
				booking.Status = BookingStatus.Processing;
			}

			await _dbContext.SaveChangesAsync();

			return pending;
		}
		finally
		{
			_semaphore.Release();
		}
	}

	public async Task UpdateBookingAsync(Booking booking)
	{
		_dbContext.Bookings.Update(booking);
		await _dbContext.SaveChangesAsync();
	}
}

}