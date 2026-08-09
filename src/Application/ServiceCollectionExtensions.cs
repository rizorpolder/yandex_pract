using Application.Services.Abstraction.Services;
using Application.Services.BackgroundBookingService;
using Application.Services.BookingService;
using Microsoft.Extensions.DependencyInjection;
using yandex_pract.CustomEventService;

namespace Application;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddApplication(this IServiceCollection services)
	{
		services.AddScoped<IEventService, EventService>();
		services.AddScoped<IBookingService, BookingService>();
		services.AddHostedService<BackgroundBookingService>();
		return services;
	}
}