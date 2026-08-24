using Application.Services.Abstraction.Services;
using Application.Services.EventBackgroundService;
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
		services.AddHostedService<BackgroundEventService>();
		return services;
	}
}