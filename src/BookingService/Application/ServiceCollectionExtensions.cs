using BookingService.Application.Services.Abstraction.Services;
using Microsoft.Extensions.DependencyInjection;

namespace BookingService.Application;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddApplication(this IServiceCollection services)
	{
		services.AddScoped<IBookingService, Services.Booking.BookingService>();
		return services;
	}
}