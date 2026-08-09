using Application.Services.Abstraction.Repositories;
using Application.Services.Abstraction.Services;
using Application.Services.BackgroundBookingService;
using Application.Services.BookingService;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Presentation.Controllers;
using yandex_pract.CustomEventService;
using yandex_pract.CustomEventService.Controllers;
using yandex_pract.DbContext;
using yandex_pract.Filters;

public class ProgramAdapter
{
	public void ConfigureServices(IServiceCollection services)
	{
		services
			.AddControllers()
			.AddApplicationPart(typeof(EventsController).Assembly);

		services.AddDbContext<AppDbContext>(options =>
			options.UseInMemoryDatabase("AppDb"));

		services.AddScoped<IEventRepository, EfEventRepository>();
		services.AddScoped<IBookingRepository, EfBookingRepository>();

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