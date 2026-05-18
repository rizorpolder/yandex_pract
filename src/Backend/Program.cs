using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using yandex_pract.CustomEventService;
using yandex_pract.Filters;
using yandex_pract.Middleware;
using yandex_pract.MockDB;
using yandex_pract.Services.BackgroundBookingService;
using yandex_pract.Services.BookingService;

public class Program
{
	public static void ConfigureServices(IServiceCollection services)
	{
		services.AddControllers();

		services.AddSingleton<MockDb>();
		services.AddSingleton<IEventDataBase>(sp => sp.GetRequiredService<MockDb>());
		services.AddSingleton<IBookingDataBase>(sp => sp.GetRequiredService<MockDb>());

		services.AddSingleton<IBookingService, BookingService>();
		
		services.AddScoped<EventFilterService>();
		services.AddScoped<IEventService, EventService>();
		
		//booking
		services.AddHostedService<BackgroundBookingService>();
		
		services.AddSwaggerGen();
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
		app.MapControllers();
	}

	public static void Main(string[] args)
	{
		var builder = WebApplication.CreateBuilder(args);
		ConfigureServices(builder.Services);

		var app = builder.Build();
		Configure(app);

		app.Run();
	}
}