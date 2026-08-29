using EventsService.Application.Options;
using EventsService.Application.Services.Abstraction.Caching;
using EventsService.Application.Services.Abstraction.Repositories;
using EventsService.Application.Services.Options;
using EventsService.Infrastructure.Caching;
using EventsService.Infrastructure.Contexts;
using EventsService.Infrastructure.Messaging;
using EventsService.Infrastructure.Options;
using EventsService.Infrastructure.Repositories;
using Infrastructure.Interceptors;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

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

		services.Configure<CacheOptions>(configuration.GetSection("Cache"));
		services.Configure<KafkaOptions>(configuration.GetSection("Kafka"));
		services.Configure<RedisOptions>(configuration.GetSection("Redis"));

		services.AddSingleton<IConnectionMultiplexer>(sp =>
		{
			var redisOptions = sp.GetRequiredService<IOptions<RedisOptions>>().Value;
			var configOptions = new ConfigurationOptions
			{
				EndPoints = {$"{redisOptions.Host}:{redisOptions.Port}"},
				Password = redisOptions.Password,
				ConnectTimeout = redisOptions.ConnectTimeoutMs,
				SyncTimeout = redisOptions.SyncTimeoutMs,
				AbortOnConnectFail = false
			};
			return ConnectionMultiplexer.Connect(configOptions);
		});

		services.AddScoped<ICacheService, RedisCacheService>();
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