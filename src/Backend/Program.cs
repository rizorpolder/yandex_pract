using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Swashbuckle.AspNetCore.SwaggerGen;
using yandex_pract.CustomEventService;
using yandex_pract.DbContext;
using yandex_pract.DbContext.Interfaces;
using yandex_pract.Filters;
using yandex_pract.Middleware;
using yandex_pract.Services.BackgroundBookingService;
using yandex_pract.Services.BookingService;
using yandex_pract.Services.Endpoints;

public class Program
{
	private static void ConfigureServerPart(WebApplicationBuilder builderServices)
	{
		var connectionString = builderServices.Configuration.GetConnectionString("DefaultConnection");
		builderServices.Services.AddDbContext<AppDbContext>(options =>
				options.UseNpgsql(connectionString)
			// .LogTo(Console.WriteLine, LogLevel.Information)   // Лог SQL запросов
			// .EnableDetailedErrors()                           // Подробный лог запросов
			// .EnableSensitiveDataLogging());                   // Самый подробрный лог для запросов, содержит критические данные 
		);
	}

	public static void ConfigureServices(IServiceCollection services)
	{
		services.AddControllers();

		services.AddScoped<IEventDataBase, EfEventDataBase>();
		services.AddScoped<IBookingDataBase, EfBookingDataBase>();

		services.AddScoped<EventFilterService>();


		services.AddScoped<IBookingService, BookingService>();
		services.AddScoped<IEventService, EventService>();

		services.AddHostedService<BackgroundBookingService>();

		services.AddSwaggerGen(options =>
		{
			options.CustomOperationIds(apiDesc => 
				apiDesc.TryGetMethodInfo(out var methodInfo) ? methodInfo.Name : null);
		});
}

	public static void Configure(WebApplication app)
	{
		app.UseMiddleware<MyCustomMiddleware>();

		if (app.Environment.IsDevelopment())
		{
			app.UseSwagger();
			app.UseSwaggerUI();
		}

		app.UseHttpsRedirection();
		app.UseRouting();

		using (var scope = app.Services.CreateScope())
		{
			var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
			db.Database.EnsureCreated();
		}

		app.MapControllers();
		//app.MapEventsEndpoints(); - так тоже можно мапить эндпойнты
	}

	public static void Main(string[] args)
	{
		var builder = WebApplication.CreateBuilder(args);

		ConfigureServices(builder.Services);
		ConfigureServerPart(builder);

		var app = builder.Build();
		Configure(app);

		app.Run();
	}
}