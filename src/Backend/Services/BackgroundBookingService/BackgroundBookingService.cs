using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using yandex_pract.MockDB;
using yandex_pract.Services.BookingService.Models;

namespace yandex_pract.Services.BackgroundBookingService;

public class BackgroundBookingService : BackgroundService
{
	private readonly IBookingDataBase _bookingDataBase;
	private readonly IEventDataBase _eventDataBase;

	private readonly SemaphoreSlim _processingSemaphore = new(1, 1);

	public BackgroundBookingService(
		IBookingDataBase bookingDataBase,
		IEventDataBase eventDataBase)
	{
		_bookingDataBase = bookingDataBase;
		_eventDataBase = eventDataBase;
	}

	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		while (!stoppingToken.IsCancellationRequested)
		{
			var pending = _bookingDataBase.GetPending().ToList();
			if (pending.Count == 0)
			{
				await Task.Delay(50, stoppingToken);
				continue;
			}

			var tasks = pending.Select(b => ProcessBookingAsync(b, stoppingToken));
			await Task.WhenAll(tasks);
		}
	}

	private async Task ProcessBookingAsync(Booking booking, CancellationToken stoppingToken)
	{
		try
		{
			await Task.Delay(10, stoppingToken);
			await _processingSemaphore.WaitAsync(stoppingToken);
			var (hasEvent, evt) = _eventDataBase.GetEventById(booking.EventId);

			if (!hasEvent)
			{
				booking.Reject();
				_bookingDataBase.UpdateBooking(booking);
				return;
			}

			booking.Confirm();
			_bookingDataBase.UpdateBooking(booking);
			_eventDataBase.Update(evt);
		}
		catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
		{
			booking.Reject();
			_bookingDataBase.UpdateBooking(booking);
		}
		catch (Exception)
		{
			booking.Reject();
			_bookingDataBase.UpdateBooking(booking);

			var (hasEvent, evt) = _eventDataBase.GetEventById(booking.EventId);
			if (hasEvent)
			{
				evt.ReleaseSeats();
				_eventDataBase.Update(evt);
			}
		}
		finally
		{
			_processingSemaphore.Release();
		}
	}
}