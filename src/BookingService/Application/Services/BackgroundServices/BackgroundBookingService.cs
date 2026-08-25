using BookingService.Application.Services.Abstraction.Broker;
using BookingService.Application.Services.Abstraction.Repositories;
using Contracts.Events;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BookingService.Application.Services.BackgroundServices;

internal class BackgroundBookingService(IServiceScopeFactory scopeFactory, ILogger<BackgroundBookingService> logger)
	: BackgroundService
{
	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		while (!stoppingToken.IsCancellationRequested)
		{
			using var scope = scopeFactory.CreateScope();
			var bookingRepository = scope.ServiceProvider.GetRequiredService<IBookingRepository>();
			var publisher = scope.ServiceProvider.GetRequiredService<IBookingConfirmedPublisher>();

			var pendingBookings = await bookingRepository.GetPendingAsync();

			foreach (var booking in pendingBookings)
			{
				try
				{
					booking.Confirm();
					await bookingRepository.SaveChangesAsync();

					await publisher.PublishAsync(new BookingConfirmed(
							booking.Id,
							booking.EventId,
							booking.UserId,
							SeatsCount: 1,
							ConfirmedAt: DateTime.UtcNow),
						stoppingToken);
				}
				catch (Exception ex)
				{
					logger.LogError(ex, "Не удалось подтвердить/опубликовать бронь {BookingId}", booking.Id);
				}
			}

			await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
		}
	}
}