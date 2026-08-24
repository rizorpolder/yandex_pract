using BookingService.Application.Services.Abstraction.Broker;
using BookingService.Application.Services.Abstraction.Repositories;
using BookingService.Infrastructure.Contexts;
using BookingService.Infrastructure.Messaging;
using BookingService.Infrastructure.Options;
using BookingService.Infrastructure.Repositories;
using Confluent.Kafka;
using Infrastructure.Interceptors;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace BookingService.Infrastructure;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
	{
		var connectionString = configuration.GetConnectionString("DefaultConnection");
		services.Configure<KafkaOptions>(configuration.GetSection("Kafka"));
		services.AddSingleton<IProducer<string, string>>(sp =>
		{
			var opts = sp.GetRequiredService<IOptions<KafkaOptions>>().Value;
			return new ProducerBuilder<string,string>(new ProducerConfig(){BootstrapServers = opts.BootstrapServers}).Build();
		});
		services.AddDbContext<AppDbContext>(options =>
		{
			options.UseNpgsql(connectionString);
			options.AddInterceptors(new DateTimeInterceptor());
		});

		services.AddScoped<IBookingRepository, EfBookingRepository>();
		services.AddScoped<IBookingRequestPublisher, KafkaBookingRequestPublisher>();
		services.AddHostedService<SeatsReservationResultConsumer>();
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