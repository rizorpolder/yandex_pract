using System.Runtime.CompilerServices;
using Application.Services.Abstraction.Repositories;
using Domain.Models.Booking;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Application.Services.BackgroundBookingService;

internal class BackgroundBookingService(IServiceScopeFactory scopeFactory) : BackgroundService
{
	private readonly SemaphoreSlim _processingSemaphore = new(1, 1);

	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		while (!stoppingToken.IsCancellationRequested)
		{
			using var scope = scopeFactory.CreateScope();

			var bookingDb = scope.ServiceProvider.GetRequiredService<IBookingRepository>();
			var eventDb = scope.ServiceProvider.GetRequiredService<IEventRepository>();

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
		IBookingRepository bookingRepository,
		IEventRepository eventRepository,
		CancellationToken stoppingToken)
	{
		try
		{
			await Task.Delay(10, stoppingToken);
			await _processingSemaphore.WaitAsync(stoppingToken);

			var evt = await eventRepository.GetByIdAsync(booking.EventId);

			if (evt is null)
			{
				booking.Reject();
				await bookingRepository.SaveChangesAsync();
				return;
			}

			booking.Confirm();
			await bookingRepository.SaveChangesAsync();
			await eventRepository.SaveChangesAsync();
		}
		catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
		{
			booking.Reject();
			await bookingRepository.SaveChangesAsync();
			await ReleaseSeatsAsync(booking, eventRepository);
		}
		catch (Exception)
		{
			booking.Reject();
			await bookingRepository.SaveChangesAsync();
			await ReleaseSeatsAsync(booking, eventRepository);
		}
		finally
		{
			_processingSemaphore.Release();
		}
	}

	private async Task ReleaseSeatsAsync(Booking booking, IEventRepository eventDb)
	{
		var evt = await eventDb.GetByIdAsync(booking.EventId);
		if (evt is not null)
		{
			evt.ReleaseSeats();
			await eventDb.SaveChangesAsync();
		}
	}
}