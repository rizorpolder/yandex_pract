using Application.Services.Abstraction.Services;
using Application.Services.BookingService;
using Microsoft.Extensions.DependencyInjection;
using yandex_pract.CustomEventService;

namespace Presentation;

public static class ApplicationRegistrationExtensions
{
	public static IServiceCollection AddApplication(this IServiceCollection services)
	{
		services.AddScoped<IEventService, EventService>();
		services.AddScoped<IBookingService, BookingService>();
		
		return services;
	}
}