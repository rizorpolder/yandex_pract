using Application.Services.Abstraction.Services;
using Application.Services.BackgroundBookingService;
using Application.Services.BookingService;
using Application.Services.EventService;
using Application.Services.Filters;
using Application.Services.UserService;
using Microsoft.Extensions.DependencyInjection;

namespace Application;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddApplication(this IServiceCollection services)
	{
		services.AddScoped<EventFilterService>();
		services.AddScoped<IEventService, EventService>();
		services.AddScoped<IBookingService, BookingService>();
		services.AddScoped<IUserService, UserService>();
		services.AddHostedService<BackgroundBookingService>();
		return services;
	}
}