using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using yandex_pract.CustomEventService;
using yandex_pract.CustomEventService.Controllers;
using yandex_pract.DbContext;
using yandex_pract.DbContext.Interfaces;
using yandex_pract.Filters;
using yandex_pract.Services.BackgroundBookingService;
using yandex_pract.Services.BookingService;

public class ProgramAdapter
{
	public void ConfigureServices(IServiceCollection services)
	{
		services
			.AddControllers()
			.AddApplicationPart(typeof(EventsController).Assembly);

		services.AddDbContext<AppDbContext>(options =>
			options.UseInMemoryDatabase("AppDb"));

		services.AddScoped<IEventDataBase, EfEventDataBase>();
		services.AddScoped<IBookingDataBase, EfBookingDataBase>();

		services.AddScoped<EventFilterService>();

		services.AddScoped<IEventService, EventService>();
		services.AddScoped<IBookingService, BookingService>();

		services.AddHostedService<BackgroundBookingService>();
	}

	public void Configure(IApplicationBuilder app)
	{
		app.UseRouting();

		app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
	}
}