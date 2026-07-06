using System.Reflection;
using Microsoft.EntityFrameworkCore;
using yandex_pract.CustomEventService.Models;
using yandex_pract.Services.BookingService.Models;

namespace yandex_pract.DbContext;

public class AppDbContext : Microsoft.EntityFrameworkCore.DbContext
{
	public DbSet<Booking> Bookings => Set<Booking>();
	public DbSet<Event> Events => Set<Event>();

	public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
	{
	}

	protected override void OnModelCreating(ModelBuilder builder)
	{
		builder.ApplyConfigurationsFromAssembly(Assembly
			.GetExecutingAssembly()); // получить все настройки из Configurations текущей сборки
		builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
		
	}
}