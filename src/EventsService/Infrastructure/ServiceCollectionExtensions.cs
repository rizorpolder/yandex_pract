using EventsService.Application.Services.Abstraction.Repositories;
using EventsService.Infrastructure.Contexts;
using EventsService.Infrastructure.Messaging;
using EventsService.Infrastructure.Options;
using EventsService.Infrastructure.Repositories;
using Infrastructure.Interceptors;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EventsService.Infrastructure;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
	{
		var connectionString = configuration.GetConnectionString("DefaultConnection");

		services.AddDbContext<AppDbContext>(options =>
		{
			options.UseNpgsql(connectionString);
			options.AddInterceptors(new DateTimeInterceptor());
		});
		
		services.Configure<KafkaOptions>(configuration.GetSection("Kafka"));
		services.AddScoped<IEventRepository, EfEventRepository>();
		services.AddScoped<IProcessedMessageRepository, EfProcessedMessageRepository>();
		services.AddHostedService<KafkaTopicInitializer>();
		services.AddHostedService<BookingConfirmedConsumer>();

		return services;
	}

	public static IApplicationBuilder UseInfrastructure(this IApplicationBuilder app)
	{
		using var scope = app.ApplicationServices.CreateScope();
		var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
		db.Database.Migrate();

		return app;
	}
}