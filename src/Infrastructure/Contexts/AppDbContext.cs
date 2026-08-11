using System.Reflection;
using Domain.Models.Booking;
using Domain.Models.Event;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Contexts;

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