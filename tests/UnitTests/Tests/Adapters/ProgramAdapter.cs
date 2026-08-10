// using Microsoft.AspNetCore.Builder;
// using Microsoft.EntityFrameworkCore;
// using Microsoft.Extensions.DependencyInjection;
// using Microsoft.Extensions.Hosting;
//
// // Presentation
// using Presentation.Controllers;
//
// // Application
// using Application.Services;
// using Application.Filters;
// using Application.Services.Abstraction.Repositories;
// using Application.Services.Abstraction.Services;
// using Application.Services.BookingService;
//
// // Infrastructure
// using Infrastructure.Db;
// using Infrastructure.Repositories;
// using Infrastructure.Background;
// using yandex_pract.CustomEventService;
// using yandex_pract.Filters;
//
// public class ProgramAdapter
// {
// 	public void ConfigureServices(IServiceCollection services)
// 	{
// 		services
// 			.AddControllers()
// 			.AddApplicationPart(typeof(EventsController).Assembly);
//
// 		// Infrastructure: EF Core
// 		services.AddDbContext<AppDbContext>(options =>
// 			options.UseNpgsql("YourConnectionString"));
//
// 		// Infrastructure: Repositories
// 		services.AddScoped<IEventRepository, EfEventRepository>();
// 		services.AddScoped<IBookingRepository, EfBookingRepository>();
//
// 		// Application: Filters
// 		services.AddScoped<EventFilterService>();
//
// 		// Application: Services
// 		services.AddScoped<IEventService, EventService>();
// 		services.AddScoped<IBookingService, BookingService>();
//
// 		// Infrastructure: Background worker
// 		services.AddHostedService<BackgroundBookingService>();
// 	}
//
// 	public void Configure(IApplicationBuilder app)
// 	{
// 		app.UseRouting();
//
// 		app.UseEndpoints(endpoints =>
// 		{
// 			endpoints.MapControllers();
// 		});
// 	}
// }