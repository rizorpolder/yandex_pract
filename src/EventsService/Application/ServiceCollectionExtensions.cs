using EventsService.Application.Services.Abstraction.Services;
using EventsService.Application.Services.EventService;
using EventsService.Application.Services.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace EventsService.Application;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddApplication(this IServiceCollection services)
	{
		services.AddScoped<EventFilterService>();
		services.AddScoped<IEventService, EventService>();
		return services;
	}
}