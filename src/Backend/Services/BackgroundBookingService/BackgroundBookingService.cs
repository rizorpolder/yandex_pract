using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using yandex_pract.MockDB;
using yandex_pract.Services.BookingService.Models;

namespace yandex_pract.Services.BackgroundBookingService;

public class BackgroundBookingService(IBookingDataBase db) : BackgroundService
{
	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		while (!stoppingToken.IsCancellationRequested)
		{
			try
			{
				var booking = db.Dequeue();
				if (booking == null)
				{
					await Task.Delay(10, stoppingToken);
					continue;
				}

				await Task.Delay(2, stoppingToken);
				UpdateState(booking);
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
	
	public void UpdateState(Booking booking)
	{
		booking.Status = BookingStatus.Confirmed;
		booking.ProceedAt = DateTime.UtcNow;
	}

}