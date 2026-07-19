using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using yandex_pract.DbContext.Interfaces;
using yandex_pract.Services.BookingService.Models;

namespace yandex_pract.Services.BackgroundBookingService;

public class BackgroundBookingService : BackgroundService
{
	private readonly IServiceScopeFactory _scopeFactory;

	private readonly SemaphoreSlim _processingSemaphore = new(1, 1);

	public BackgroundBookingService(IServiceScopeFactory scopeFactory)
	{
		_scopeFactory = scopeFactory;
	}

	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		while (!stoppingToken.IsCancellationRequested)
		{
			using var scope = _scopeFactory.CreateScope();

			var bookingDb = scope.ServiceProvider.GetRequiredService<IBookingDataBase>();
			var eventDb = scope.ServiceProvider.GetRequiredService<IEventDataBase>();

			var pending = await bookingDb.GetPendingAsync();
			if (pending.Count == 0)
			{
				await Task.Delay(50, stoppingToken);
				continue;
			}

			var tasks = pending.Select(b => ProcessBookingAsync(b, bookingDb, eventDb, stoppingToken));
			await Task.WhenAll(tasks);
		}
	}

	private async Task ProcessBookingAsync(
		Booking booking,
		IBookingDataBase bookingDb,
		IEventDataBase eventDb,
		CancellationToken stoppingToken)
	{
		try
		{
			await Task.Delay(10, stoppingToken);
			await _processingSemaphore.WaitAsync(stoppingToken);

			var (hasEvent, evt) = await eventDb.GetEventByIdAsync(booking.EventId);

			if (!hasEvent)
			{
				booking.Reject();
				await bookingDb.UpdateBookingAsync(booking);
				return;
			}

			booking.Confirm();
			await bookingDb.UpdateBookingAsync(booking);
			await eventDb.UpdateAsync(evt);
		}
		catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
		{
			booking.Reject();
			await bookingDb.UpdateBookingAsync(booking);
			await ReleaseSeatsAsync(booking, eventDb);
		}
		catch (Exception)
		{
			booking.Reject();
			await bookingDb.UpdateBookingAsync(booking);
			await ReleaseSeatsAsync(booking, eventDb);
		}
		finally
		{
			_processingSemaphore.Release();
		}
	}

	private async Task ReleaseSeatsAsync(Booking booking, IEventDataBase eventDb)
	{
		var (hasEvent, evt) = await eventDb.GetEventByIdAsync(booking.EventId);
		if (hasEvent)
		{
			evt.ReleaseSeats();
			await eventDb.UpdateAsync(evt);
		}
	}
}