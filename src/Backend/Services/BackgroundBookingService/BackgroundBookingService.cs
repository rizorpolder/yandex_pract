using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using yandex_pract.MockDB;
using yandex_pract.Services.BookingService.Modesl;

namespace yandex_pract.Services.BackgroundBookingService;

public class BackgroundBookingService(IBookingDataBase db) : BackgroundService
{
	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		while (!stoppingToken.IsCancellationRequested)
		{
			try
			{
				var (hasBooking, booking) = db.TryDequeue();
				if (!hasBooking)
					continue;
				
				await Task.Delay(2, stoppingToken);
				booking.Status = BookingStatus.Confirmed;
				booking.ProceedAt = DateTime.UtcNow;
				db.TryAddBooking(booking.EventId, booking);
			}
			catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
			{
				break;
			}
			catch (Exception ex)
			{
				//Other exception
			}
		}
	}
}