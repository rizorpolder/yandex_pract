using Application.Services.Abstraction.Services;
using Application.Services.BackgroundBookingService;
using Application.Services.BookingService;
using Application.Services.EventService;
using Application.Services.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace Application;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddApplication(this IServiceCollection services)
	{
		services.AddScoped<EventFilterService>();
		services.AddScoped<IEventService, EventService>();
		services.AddScoped<IBookingService, BookingService>();
		services.AddHostedService<BackgroundBookingService>();
		return services;
	}
}