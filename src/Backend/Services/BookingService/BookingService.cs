using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using yandex_pract.CustomException;
using yandex_pract.DbContext;
using yandex_pract.DbContext.Interfaces;
using yandex_pract.Services.BookingService.Models;

namespace yandex_pract.Services.BookingService;

public class BookingService(IBookingRepository bookingRepository, IEventRepository eventRepository)
	: IBookingService
{
	private readonly SemaphoreSlim _semaphore = new(1, 1);

	public async Task<(bool result, Booking booking)> CreateBookingAsync(Guid eventId)
	{
		await _semaphore.WaitAsync();
		try
		{
			var (hasEvent, eventData) = await eventRepository.GetEventByIdAsync(eventId);
			if (!hasEvent)
				return (false, null);

			if (!eventData.TryReserveSeats())
				throw new NoAvailableSeatsException("No available seats");

			await eventRepository.UpdateAsync(eventData);

			var booking = new Booking(eventId);

			bool isSuccess = await bookingRepository.EnqueueAsync(booking);
			
			return (isSuccess, booking);
		}
		finally
		{
			_semaphore.Release();
		}
	}

	public async Task<(bool haveBooking, Booking booking)> GetBookingByIdAsync(Guid bookingId)
	{
		return await bookingRepository.TryFindBookingAsync(bookingId);
	}

	public async Task<List<Booking>> DequeuePendingAsync()
	{
		await _semaphore.WaitAsync();
		try
		{
			var pending = await bookingRepository.GetPendingAsync();

			foreach (var booking in pending)
			{
				booking.Status = BookingStatus.Processing;
				await bookingRepository.UpdateBookingAsync(booking);
			}
			
			return pending;
		}
		finally
		{
			_semaphore.Release();
		}
	}
}