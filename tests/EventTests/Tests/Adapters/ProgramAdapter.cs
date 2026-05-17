using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using TestProject.Tests.Database;
using yandex_pract.CustomEventService;
using yandex_pract.CustomEventService.Controllers;
using yandex_pract.Filters;
using yandex_pract.MockDB;

public class ProgramAdapter
{
	public void ConfigureServices(IServiceCollection services)
	{
		services
			.AddControllers()
			.AddApplicationPart(typeof(EventsController).Assembly);

		services.AddSingleton<IEventDataBase, TestDB>();
		services.AddScoped<EventFilterService>();
		services.AddScoped<IEventService, EventService>();
	}

	public void Configure(IApplicationBuilder app)
	{
		app.UseRouting();

		app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
	}
}